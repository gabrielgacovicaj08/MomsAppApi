using Microsoft.Data.SqlClient;
using MomsAppApi.Models.WorkLogDTO;
using System.Data;

namespace MomsAppApi.Services.WorkLogService
{
    public class WorkLogService(IConfiguration configuration) : IWorkLogService
    {

        private SqlConnection NewConn() => new SqlConnection(configuration.GetConnectionString("MomsAppDb"));

        public async Task<Boolean> CreateWorkLog(WorkLogRequestDTO request)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.CreateWorkLog", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@assignment_id", SqlDbType.Int).Value = request.assignment_id;
            cmd.Parameters.Add("@started_at", SqlDbType.DateTime).Value = request.started_at;
            cmd.Parameters.Add("@ended_at", SqlDbType.DateTime).Value = request.ended_at;
            cmd.Parameters.Add("@notes", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(request.notes) ? DBNull.Value : request.notes;

            try
            {
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex) 
            {
                Logger.LogError("Couldn't upload the WorkLog: ", ex);
                return false;
            }
        }
    }
}
