using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using MomsAppApi.Models.AssignmentDTO;
using System.Data;
using System.Threading;

namespace MomsAppApi.Services.AssignmentService
{
    public class AssignmentService(IConfiguration configuration, IMemoryCache cache) : IAssignmentService
    {
        private static int _cacheVersion;
        private static readonly TimeSpan AssignmentCacheTtl = TimeSpan.FromSeconds(90);

        private static string AssignmentsByDayCacheKey(DateOnly date) => $"assignments:day:{date:yyyy-MM-dd}:v{Volatile.Read(ref _cacheVersion)}";
        private static string AssignmentsByEmployeeCacheKey(int employeeId) => $"assignments:employee:{employeeId}:v{Volatile.Read(ref _cacheVersion)}";

        private static void BumpCacheVersion() => Interlocked.Increment(ref _cacheVersion);

        private SqlConnection NewConn() => new SqlConnection(configuration.GetConnectionString("MomsAppDb"));

        public async Task<CreateAssignmentDTO?> CreateAssignmentAsync(CreateAssignmentDTO request)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.CreateAssignment", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("work_date", SqlDbType.Date).Value = request.work_date;
                cmd.Parameters.Add("employee_id", SqlDbType.Int).Value = request.employee_id;
                cmd.Parameters.Add("structure_id", SqlDbType.Int).Value = request.structure_id;
                cmd.Parameters.Add("@shift_start", SqlDbType.Time).Value =
                    request.shift_start.HasValue ? (object)request.shift_start.Value : DBNull.Value;

                cmd.Parameters.Add("@shift_end", SqlDbType.Time).Value =
                    request.shift_end.HasValue ? (object)request.shift_end.Value : DBNull.Value;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                BumpCacheVersion();
                return request;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error creating assignment", ex);
                return null;
            }
        }

        public async Task<bool> UpdateAssignmentAsync(int assignment_id, UpdateAssignmentDTO request)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.UpdateAssignment", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@assignment_id", SqlDbType.Int).Value = assignment_id;
                cmd.Parameters.Add("@work_date", SqlDbType.Date).Value = request.work_date;
                cmd.Parameters.Add("@employee_id", SqlDbType.Int).Value = request.employee_id;
                cmd.Parameters.Add("@structure_id", SqlDbType.Int).Value = request.structure_id;
                cmd.Parameters.Add("@shift_start", SqlDbType.Time).Value =
                    request.shift_start.HasValue ? (object)request.shift_start.Value : DBNull.Value;
                cmd.Parameters.Add("@shift_end", SqlDbType.Time).Value =
                    request.shift_end.HasValue ? (object)request.shift_end.Value : DBNull.Value;
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = request.status.Trim().ToUpperInvariant();

                await conn.OpenAsync();
                var rowsUpdated = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (rowsUpdated > 0)
                {
                    BumpCacheVersion();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error updating assignment with ID {assignment_id}", ex);
                return false;
            }
        }

        public async Task<List<ResponseAssignmentDTO?>> GetAllAssignmentsByDay(DateOnly date)
        {
            var cacheKey = AssignmentsByDayCacheKey(date);
            if (cache.TryGetValue(cacheKey, out List<ResponseAssignmentDTO?>? cachedAssignments) && cachedAssignments is not null)
            {
                return cachedAssignments;
            }

            var assignments = new List<ResponseAssignmentDTO>();

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.GetAllAssignmentsByDay", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("work_date", SqlDbType.Date).Value = date;

                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var assignment = new ResponseAssignmentDTO
                    {
                        assignment_id = reader.GetInt32(reader.GetOrdinal("assignment_id")),
                        work_date = date,
                        shift_start = reader.IsDBNull(reader.GetOrdinal("shift_start")) ? null : TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("shift_start"))),
                        shift_end = reader.IsDBNull(reader.GetOrdinal("shift_end")) ? null : TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("shift_end"))),
                        first_name = reader.GetString(reader.GetOrdinal("first_name")),
                        last_name = reader.GetString(reader.GetOrdinal("last_name")),
                        structure_name = reader.GetString(reader.GetOrdinal("HotelName")),
                        status = reader.GetString(reader.GetOrdinal("status"))
                    };
                    assignments.Add(assignment);
                }

                cache.Set(cacheKey, assignments, AssignmentCacheTtl);
                return assignments;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error retrieving assignments", ex);
                return null;
            }
        }

        public async Task<List<ResponseAssignmentDTO?>> GetAssignementsByEmpId(int employee_id)
        {
            var cacheKey = AssignmentsByEmployeeCacheKey(employee_id);
            if (cache.TryGetValue(cacheKey, out List<ResponseAssignmentDTO?>? cachedAssignments) && cachedAssignments is not null)
            {
                return cachedAssignments;
            }

            var assignements = new List<ResponseAssignmentDTO>();

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.GetAssignmentsByEmpId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@employee_id", SqlDbType.Int).Value = employee_id;

                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var assignement = new ResponseAssignmentDTO
                    {
                        assignment_id = reader.GetInt32(reader.GetOrdinal("assignment_id")),
                        work_date = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("work_date"))),
                        shift_start = reader.IsDBNull(reader.GetOrdinal("shift_start")) ? null : TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("shift_start"))),
                        shift_end = reader.IsDBNull(reader.GetOrdinal("shift_end")) ? null : TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("shift_end"))),
                        first_name = reader.GetString(reader.GetOrdinal("first_name")),
                        last_name = reader.GetString(reader.GetOrdinal("last_name")),
                        structure_name = reader.GetString(reader.GetOrdinal("HotelName")),
                        status = reader.GetString(reader.GetOrdinal("status"))
                    };

                    assignements.Add(assignement);
                }

                cache.Set(cacheKey, assignements, AssignmentCacheTtl);
                return assignements;
            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't find any assignment for this employee ", ex);
                return null;
            }
        }
    }
}
