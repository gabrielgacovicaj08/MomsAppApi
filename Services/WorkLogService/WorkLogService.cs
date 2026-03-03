using Microsoft.Data.SqlClient;
using MomsAppApi.Models.WorkLogDTO;
using System.Data;

namespace MomsAppApi.Services.WorkLogService
{
    public class WorkLogService(IConfiguration configuration) : IWorkLogService
    {
        private TimeSpan MaxShiftDuration => TimeSpan.FromHours(configuration.GetValue<double?>("WorkLog:MaxShiftHours") ?? 16);
        private TimeSpan FutureClockSkewAllowance => TimeSpan.FromMinutes(configuration.GetValue<double?>("WorkLog:FutureClockSkewMinutes") ?? 10);

        private SqlConnection NewConn() => new SqlConnection(configuration.GetConnectionString("MomsAppDb"));

        public async Task<Boolean> CreateWorkLog(WorkLogRequestDTO request)
        {
            var startedUtc = request.started_at.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.started_at, DateTimeKind.Utc)
                : request.started_at.ToUniversalTime();

            var endedUtc = request.ended_at.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.ended_at, DateTimeKind.Utc)
                : request.ended_at.ToUniversalTime();

            var duration = endedUtc - startedUtc;
            var nowUtc = DateTime.UtcNow;

            if (request.assignment_id <= 0 ||
                endedUtc <= startedUtc ||
                duration > MaxShiftDuration ||
                startedUtc > nowUtc.Add(FutureClockSkewAllowance) ||
                endedUtc > nowUtc.Add(FutureClockSkewAllowance))
            {
                return false;
            }

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.CreateWorkLog", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@assignment_id", SqlDbType.Int).Value = request.assignment_id;
            cmd.Parameters.Add("@started_at", SqlDbType.DateTime).Value = startedUtc;
            cmd.Parameters.Add("@ended_at", SqlDbType.DateTime).Value = endedUtc;
            cmd.Parameters.Add("@notes", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(request.notes)
                ? DBNull.Value
                : request.notes.Trim();

            try
            {
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't upload the WorkLog.", ex);
                return false;
            }
        }
    }
}
