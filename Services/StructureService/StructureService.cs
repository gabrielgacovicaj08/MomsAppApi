using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using MomsAppApi.Models.StructuresDTO;
using System.Data;
using System.Threading;

namespace MomsAppApi.Services.StructureService
{
    public class StructureService(IConfiguration configuration, IMemoryCache cache) : IStructureService
    {
        private static int _cacheVersion;
        private static readonly TimeSpan StructureCacheTtl = TimeSpan.FromMinutes(5);

        private static string AllStructuresCacheKey => $"structures:all:v{Volatile.Read(ref _cacheVersion)}";
        private static string StructureByIdCacheKey(int structureId) => $"structures:id:{structureId}:v{Volatile.Read(ref _cacheVersion)}";

        private static void BumpCacheVersion() => Interlocked.Increment(ref _cacheVersion);

        private SqlConnection NewConn() => new SqlConnection(configuration.GetConnectionString("MomsAppDb"));

        public async Task<CreateStructureDTO?> CreateStructureAsync(CreateStructureDTO request)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.CreateStructure", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = request.name;
                cmd.Parameters.Add("@address_line", SqlDbType.NVarChar, 250).Value = request.address_line;
                cmd.Parameters.Add("@city", SqlDbType.NVarChar, 100).Value = request.city;
                cmd.Parameters.Add("@zip", SqlDbType.NVarChar, 7).Value = request.zip;
                cmd.Parameters.Add("@client_name", SqlDbType.NVarChar, 150).Value = request.client_name;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                BumpCacheVersion();
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
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.UpdateStructure", conn)
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
                cmd.Parameters.Add("@is_active", SqlDbType.Bit).Value = request.is_active;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                BumpCacheVersion();
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
            var cacheKey = AllStructuresCacheKey;
            if (cache.TryGetValue(cacheKey, out List<StructureResponseDTO?>? cachedStructures) && cachedStructures is not null)
            {
                return cachedStructures;
            }

            var structures = new List<StructureResponseDTO>();

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.GetAllStructures", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    structures.Add(new StructureResponseDTO
                    {
                        structure_id = Convert.ToInt32(reader["structure_id"]),
                        name = reader["name"].ToString(),
                        address_line = reader["address_line"].ToString(),
                        city = reader["city"].ToString(),
                        zip = reader["zip"].ToString(),
                        client_name = reader["client_name"].ToString(),
                        is_active = Convert.ToBoolean(reader["is_active"])
                    });
                }

                cache.Set(cacheKey, structures, StructureCacheTtl);
                foreach (var structure in structures)
                {
                    cache.Set(StructureByIdCacheKey(structure.structure_id), structure, StructureCacheTtl);
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
            var cacheKey = StructureByIdCacheKey(structure_id);
            if (cache.TryGetValue(cacheKey, out StructureResponseDTO? cachedStructure))
            {
                return cachedStructure;
            }

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.GetStructureById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@structure_id", SqlDbType.Int).Value = structure_id;
                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var structure = new StructureResponseDTO
                    {
                        structure_id = structure_id,
                        name = reader["name"].ToString(),
                        address_line = reader["address_line"].ToString(),
                        city = reader["city"].ToString(),
                        zip = reader["zip"].ToString(),
                        client_name = reader["client_name"].ToString(),
                        is_active = Convert.ToBoolean(reader["is_active"])
                    };

                    cache.Set(cacheKey, structure, StructureCacheTtl);
                    return structure;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't retrieve the structure: ", ex);
                return null;
            }
        }

        public async Task<bool> DeleteStructureAsync(int structure_id)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.DeleteStructure", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                cmd.Parameters.Add("@structure_id", SqlDbType.Int).Value = structure_id;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                BumpCacheVersion();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't delete the structure: ", ex);
                return false;
            }
        }
    }
}
