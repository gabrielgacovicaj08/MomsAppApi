using Microsoft.Data.SqlClient;
using MomsAppApi.Models.AssignmentDTO;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MomsAppApi.Services.AssignmentService
{
    public class AssignmentService(IConfiguration configuration) : IAssignmentService
    {

        private SqlConnection NewConn() => new SqlConnection(configuration.GetConnectionString("MomsAppDb"));

        public async Task<CreateAssignmentDTO?> CreateAssignmentAsync(CreateAssignmentDTO request)
        {
            await using var conn = NewConn();
            using var cmd = new SqlCommand("dbo.CreateAssignment", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("work_date", System.Data.SqlDbType.Date).Value = request.work_date;
                cmd.Parameters.Add("employee_id", System.Data.SqlDbType.Int).Value = request.employee_id;
                cmd.Parameters.Add("structure_id", System.Data.SqlDbType.Int).Value = request.structure_id;
                cmd.Parameters.Add("@shift_start", SqlDbType.Time).Value =
                    request.shift_start.HasValue ? (object)request.shift_start.Value : DBNull.Value;

                cmd.Parameters.Add("@shift_end", SqlDbType.Time).Value =
                    request.shift_end.HasValue ? (object)request.shift_end.Value : DBNull.Value;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return request;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Logger.LogError("Error creating assignment", ex);
                return null;
            }
        }

        public Task<bool> UpdateAssignmentAsync(int assignment_id, UpdateAssignmentDTO request)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ResponseAssignmentDTO?>> GetAllAssignmentsByDay(DateOnly date)
        {

            var assignments = new List<ResponseAssignmentDTO>();

            using var conn = NewConn();
            using var cmd = new SqlCommand("dbo.GetAllAssignmentsByDay", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            try
            {
                cmd.Parameters.Add("work_date", System.Data.SqlDbType.Date).Value = date;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
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
                return assignments;

            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Logger.LogError("Error retrieving assignments", ex);
                return null;
            }
        }

        public async Task<List<ResponseAssignmentDTO?>> GetAssignementsByEmpId(int employee_id)
        {
            var assignements = new List<ResponseAssignmentDTO>();

            using var conn = NewConn();
            using var cmd = new SqlCommand("dbo.GetAssignmentsByEmpId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@employee_id", SqlDbType.Int).Value = employee_id;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while(reader.Read())
                {
                    var assignement = new ResponseAssignmentDTO
                    {
                        assignment_id = reader.GetInt32(reader.GetOrdinal("assignment_id")),
                        work_date = DateOnly.FromDateTime(reader.GetDateTime("work_date")),
                        shift_start = reader.IsDBNull(reader.GetOrdinal("shift_start")) ? null : TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("shift_start"))),
                        shift_end = reader.IsDBNull(reader.GetOrdinal("shift_end")) ? null : TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("shift_end"))),
                        first_name = reader.GetString(reader.GetOrdinal("first_name")),
                        last_name = reader.GetString(reader.GetOrdinal("last_name")),
                        structure_name = reader.GetString(reader.GetOrdinal("HotelName")),
                        status = reader.GetString(reader.GetOrdinal("status"))
                    };

                    assignements.Add(assignement);

                    
                }
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
