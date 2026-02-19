using Microsoft.Data.SqlClient;
using MomsAppApi.Models.AssignmentDTO;
using System.Data;

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
    }
}
