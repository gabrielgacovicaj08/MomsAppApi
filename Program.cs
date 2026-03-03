using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MomsAppApi.Data;
using MomsAppApi.Services.AssignmentService;
using MomsAppApi.Services.AuthService;
using MomsAppApi.Services.EmployeeService;
using MomsAppApi.Services.StructureService;
using MomsAppApi.Services.WorkLogService;
using Scalar.AspNetCore;
using System.IO;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

const string CorrelationIdHeader = "X-Correlation-Id";

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        context.ProblemDetails.Extensions["correlationId"] =
            context.HttpContext.Items[CorrelationIdHeader]?.ToString() ?? context.HttpContext.TraceIdentifier;
    };
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<MomsAppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MomsAppDb"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(builder.Configuration.GetValue<int?>("Database:CommandTimeoutSeconds") ?? 30);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: builder.Configuration.GetValue<int?>("Database:MaxRetryCount") ?? 5,
                maxRetryDelay: TimeSpan.FromSeconds(builder.Configuration.GetValue<int?>("Database:MaxRetryDelaySeconds") ?? 10),
                errorNumbersToAdd: null);
        }));

var jwtToken = builder.Configuration["AppSettings:Token"];
if (string.IsNullOrWhiteSpace(jwtToken))
{
    throw new InvalidOperationException("Missing AppSettings:Token configuration.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtToken))
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 8,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var configuredCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray()
    ?? [];

var allowedCorsOrigins = configuredCorsOrigins.Length > 0
    ? configuredCorsOrigins
    : ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedCorsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IStructureService, StructureService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IWorkLogService,  WorkLogService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var incomingCorrelationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
    var correlationId = string.IsNullOrWhiteSpace(incomingCorrelationId)
        ? context.TraceIdentifier
        : incomingCorrelationId.Trim();

    if (correlationId.Length > 128)
    {
        correlationId = correlationId[..128];
    }

    context.Items[CorrelationIdHeader] = correlationId;

    context.Response.OnStarting(() =>
    {
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        return Task.CompletedTask;
    });

    await next();
});

app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("RequestLogging");

    var startedAt = DateTime.UtcNow;
    var startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();

    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
        return Task.CompletedTask;
    });

    await next();

    var elapsedMs = System.Diagnostics.Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
    var userId = context.User.FindFirst("employee_id")?.Value ?? "anonymous";
    var role = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "none";

    var correlationId = context.Items[CorrelationIdHeader]?.ToString() ?? context.TraceIdentifier;

    logger.LogInformation(
        "HTTP {Method} {Path} => {StatusCode} in {ElapsedMs:0.0} ms | traceId={TraceId} | correlationId={CorrelationId} | user={UserId} | role={Role} | startedUtc={StartedUtc}",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        elapsedMs,
        context.TraceIdentifier,
        correlationId,
        userId,
        role,
        startedAt);
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");

            var correlationId = context.Items[CorrelationIdHeader]?.ToString() ?? context.TraceIdentifier;

            if (exception is not null)
            {
                logger.LogError(exception, "Unhandled exception while processing {Method} {Path} | correlationId={CorrelationId}", context.Request.Method, context.Request.Path, correlationId);
            }

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Please retry or contact support if the issue persists.",
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;
            problem.Extensions["timestamp"] = DateTime.UtcNow;
            problem.Extensions["correlationId"] = correlationId;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        });
    });

    app.UseHsts();
}

app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;

    if (response.StatusCode >= 400 &&
        response.StatusCode < 600 &&
        string.IsNullOrWhiteSpace(response.ContentType))
    {
        response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = response.StatusCode,
            Title = "Request failed.",
            Detail = $"HTTP {response.StatusCode}",
            Instance = statusCodeContext.HttpContext.Request.Path
        };
        problem.Extensions["traceId"] = statusCodeContext.HttpContext.TraceIdentifier;
        problem.Extensions["timestamp"] = DateTime.UtcNow;
        problem.Extensions["correlationId"] =
            statusCodeContext.HttpContext.Items[CorrelationIdHeader]?.ToString()
            ?? statusCodeContext.HttpContext.TraceIdentifier;

        await response.WriteAsJsonAsync(problem);
    }
});

app.UseCors("AllowFrontend");

//app.Use(async (context, next) =>
//{
//    await next();

//    var path = context.Request.Path;

//    // Skip CSP/security headers for docs UIs (dev tooling)
//    if (path.StartsWithSegments("/scalar") || path.StartsWithSegments("/openapi"))
//        return;

   

//        var headers = context.Response.Headers;
//        headers.TryAdd("X-Content-Type-Options", "nosniff");
//        headers.TryAdd("X-Frame-Options", "DENY");
//        headers.TryAdd("Referrer-Policy", "no-referrer");
//        headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
//        headers.TryAdd("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'; base-uri 'none';");


//});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        // Make Scalar look for your OpenAPI exactly where it is:
        options.WithOpenApiRoutePattern("/openapi/v1.json");
        // Optional: prettier label
        options.WithTitle("MomsApp API");
    }); ;
    app.MapOpenApi();
}

app.MapGet("/health/live", () => Results.Ok(new
{
    status = "ok",
    service = "momsappapi",
    utc = DateTime.UtcNow
}));

app.MapGet("/health/ready", async (MomsAppDbContext db, CancellationToken ct) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync(ct);
        if (!canConnect)
        {
            return Results.Problem(
                title: "Database is unreachable",
                detail: "The API is running but cannot connect to MomsAppDb.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Results.Ok(new
        {
            status = "ready",
            db = "ok",
            utc = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Readiness check failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.UseHttpsRedirection();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
