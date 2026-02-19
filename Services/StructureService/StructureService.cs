using Microsoft.Data.SqlClient;
using MomsAppApi.Models.StructuresDTO;
using System.Data;

namespace MomsAppApi.Services.StructureService
{
    public class StructureService(IConfiguration configuration) : IStructureService
    {
        private SqlConnection NewConn() => new SqlConnection(configuration.GetConnectionString("MomsAppDb"));
        public async Task<CreateStructureDTO?> CreateStructureAsync(CreateStructureDTO request)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.CreateStructure", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = request.name;
                cmd.Parameters.Add("@address_line", SqlDbType.NVarChar, 250).Value = request.address_line;
                cmd.Parameters.Add("@city", SqlDbType.NVarChar, 100).Value = request.city;
                cmd.Parameters.Add("@zip", SqlDbType.NVarChar, 7).Value = request.zip;
                cmd.Parameters.Add("@client_name", SqlDbType.NVarChar, 150).Value = request.client_name;

                await conn.OpenAsync();
                cmd.ExecuteNonQuery();

                return request;
            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't create the structure: ", ex);
                return null;
            }
        }

        public async Task<bool> UpdateStructureAsync(int structure_id, UpdateStructureDTO request)
        {
            using var conn = NewConn();
            using var cmd = new SqlCommand("dbo.UpdateStructure", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@structure_id", SqlDbType.Int).Value = structure_id;
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = request.name;
                cmd.Parameters.Add("@address_line", SqlDbType.NVarChar, 250).Value = request.address_line;
                cmd.Parameters.Add("@city", SqlDbType.NVarChar, 100).Value = request.city;
                cmd.Parameters.Add("@zip", SqlDbType.NVarChar, 7).Value = request.zip;
                cmd.Parameters.Add("@client_name", SqlDbType.NVarChar, 150).Value = request.client_name;

                await conn.OpenAsync();
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't update the structure: ", ex);
                return false;
            }
        }

        public async Task<List<StructureResponseDTO?>> GetAllStructuresAsync()
        {
            var structures = new List<StructureResponseDTO>();

            using var conn = NewConn();
            using var cmd = new SqlCommand("dbo.GetAllStructures", conn)

            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    structures.Add(new StructureResponseDTO
                    {
                        name = reader["name"].ToString(),
                        address_line = reader["address_line"].ToString(),
                        city = reader["city"].ToString(),
                        zip = reader["zip"].ToString(),
                        client_name = reader["client_name"].ToString()
                    });
                }
                return structures;
            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't retrieve the structures: ", ex);
                return null;
            }
             
        }

        public async Task<StructureResponseDTO?> GetStructureByIdAsync(int structure_id)
        {
            using var conn = NewConn();
            using var cmd = new SqlCommand("dbo.GetStructureById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@structure_id", SqlDbType.Int).Value = structure_id;
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    return new StructureResponseDTO
                    {
                        name = reader["name"].ToString(),
                        address_line = reader["address_line"].ToString(),
                        city = reader["city"].ToString(),
                        zip = reader["zip"].ToString(),
                        client_name = reader["client_name"].ToString()
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't retrieve the structure: ", ex);
                return null;
            }

        }
    }
}
