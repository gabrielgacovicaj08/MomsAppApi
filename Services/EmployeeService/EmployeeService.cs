using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MomsAppApi.Data;
using MomsAppApi.Entities;
using MomsAppApi.Models.EmployeeDTO;
using System.Data;
using System.Security.Cryptography;

namespace MomsAppApi.Services.EmployeeService
{
    public class EmployeeService(IConfiguration configuration, IMemoryCache cache) : IEmployeeService
    {
        private const string PasswordChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%^&*";
        private static readonly TimeSpan EmployeeCacheTtl = TimeSpan.FromMinutes(2);
        private const string AllEmployeesCacheKey = "employees:all";

        private static string EmployeeByIdCacheKey(int employeeId) => $"employees:id:{employeeId}";

        private SqlConnection NewConn() => new SqlConnection(configuration.GetConnectionString("MomsAppDb"));

        private void InvalidateEmployeeCache(int? employeeId = null)
        {
            cache.Remove(AllEmployeesCacheKey);
            if (employeeId.HasValue)
            {
                cache.Remove(EmployeeByIdCacheKey(employeeId.Value));
            }
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        private static string NormalizeRole(string role) => role.Trim().ToUpperInvariant();

        private static EmployeeResponseDTO WithoutTemporaryPassword(EmployeeResponseDTO employee) => new()
        {
            employee_id = employee.employee_id,
            first_name = employee.first_name,
            last_name = employee.last_name,
            email = employee.email,
            phone = employee.phone,
            role = employee.role,
            is_active = employee.is_active,
            temporary_password = null
        };

        private static string GenerateTemporaryPassword(int length = 14)
        {
            var chars = new char[length];
            for (var i = 0; i < length; i++)
            {
                chars[i] = PasswordChars[RandomNumberGenerator.GetInt32(PasswordChars.Length)];
            }

            return new string(chars);
        }

        public async Task<EmployeeResponseDTO?> CreateEmployeeAsync(CreateEmployeeDTO request)
        {
            int new_employee_id;
            var normalizedEmail = NormalizeEmail(request.email);
            var normalizedRole = NormalizeRole(request.role);

            await using var conn = NewConn();
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();


            try
            {



                await using (var cmd = new SqlCommand("dbo.CreateEmployee", conn, (SqlTransaction)transaction) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@first_name", request.first_name.Trim());
                    cmd.Parameters.AddWithValue("@last_name", request.last_name.Trim());
                    cmd.Parameters.AddWithValue("@phone", request.phone.Trim());
                    cmd.Parameters.AddWithValue("@email", normalizedEmail);
                    cmd.Parameters.AddWithValue("@role", normalizedRole);

                    object? result = await cmd.ExecuteScalarAsync();

                    if (result is null) return null;

                    new_employee_id = Convert.ToInt32(result);
                }



                //context.Employees.Add(employee);
                //await context.SaveChangesAsync(); // generates EmployeeId



                var userAccount = new UserAccount();
                var temporaryPassword = GenerateTemporaryPassword();

                var hashedPassword = new PasswordHasher<UserAccount>()
                    .HashPassword(userAccount, temporaryPassword);


                await using (var cmd = new SqlCommand("dbo.CreateUserAccount", conn, (SqlTransaction)transaction) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@employee_id", new_employee_id);
                    cmd.Parameters.AddWithValue("@email", normalizedEmail);
                    cmd.Parameters.AddWithValue("@password_hash", hashedPassword);
                    cmd.Parameters.AddWithValue("@role", normalizedRole);
                    await cmd.ExecuteNonQueryAsync();
                }



                await transaction.CommitAsync();
                var response = new EmployeeResponseDTO
                {
                    employee_id = new_employee_id,
                    first_name = request.first_name.Trim(),
                    last_name = request.last_name.Trim(),
                    email = normalizedEmail,
                    phone = request.phone.Trim(),
                    role = normalizedRole,
                    is_active = true,
                    temporary_password = temporaryPassword
                };

                InvalidateEmployeeCache(new_employee_id);
                cache.Set(EmployeeByIdCacheKey(new_employee_id), WithoutTemporaryPassword(response), EmployeeCacheTtl);

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.LogError("Error creating employee and user account", ex);
                return null;

            }


        }

        public async Task<EmployeeResponseDTO?> GetEmployeeByIdAsync(int employee_id)
        {
            var cacheKey = EmployeeByIdCacheKey(employee_id);
            if (cache.TryGetValue(cacheKey, out EmployeeResponseDTO? cachedEmployee))
            {
                return cachedEmployee;
            }

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.GetEmployeeById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@employee_id", SqlDbType.Int).Value = employee_id;
            try
            {
                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return null;

                var employee = new EmployeeResponseDTO
                {
                    employee_id = employee_id,
                    first_name = reader.GetString(reader.GetOrdinal("first_name")),
                    last_name = reader.GetString(reader.GetOrdinal("last_name")),
                    email = reader.GetString(reader.GetOrdinal("email")),
                    phone = reader.GetString(reader.GetOrdinal("phone")),
                    role = reader.GetString(reader.GetOrdinal("role")),
                    is_active = reader.GetBoolean(reader.GetOrdinal("is_active"))
                };

                cache.Set(cacheKey, employee, EmployeeCacheTtl);
                return employee;
            }
            catch (Exception ex)
            {

                Logger.LogError($"Error fetching employee with ID {employee_id}", ex);

                return null;
            }

        }

        public async Task<EmployeeResponseDTO?> UpdateEmployeeAsync(int employee_id, UpdateEmployeeRequestDTO updatedEmployee)
        {

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.UpdateEmployee", conn)
            {
                CommandType = CommandType.StoredProcedure
            };



            cmd.Parameters.AddWithValue("@employee_id", employee_id);

            cmd.Parameters.Add("@first_name", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(updatedEmployee.first_name) ? DBNull.Value : updatedEmployee.first_name.Trim();
            cmd.Parameters.Add("@last_name", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(updatedEmployee.last_name) ? DBNull.Value : updatedEmployee.last_name.Trim();
            cmd.Parameters.Add("@phone", SqlDbType.NVarChar, 30).Value = string.IsNullOrWhiteSpace(updatedEmployee.phone) ? DBNull.Value : updatedEmployee.phone.Trim();
            cmd.Parameters.Add("@email", SqlDbType.NVarChar, 256).Value = string.IsNullOrWhiteSpace(updatedEmployee.email) ? DBNull.Value : NormalizeEmail(updatedEmployee.email);
            cmd.Parameters.Add("@role", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(updatedEmployee.role) ? DBNull.Value : NormalizeRole(updatedEmployee.role);
            cmd.Parameters.Add("@is_active", SqlDbType.Bit).Value = (object?)updatedEmployee.is_active ?? DBNull.Value;

            try
            {
                await conn.OpenAsync();

                var rows = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                if (rows == 0) return null;

                InvalidateEmployeeCache(employee_id);
                return await GetEmployeeByIdAsync(employee_id);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error updating employee with ID {employee_id}", ex);
                return null;



            }
        }

        public async Task<List<EmployeeResponseDTO?>> GetAllEmployeesAsync()
        {
            if (cache.TryGetValue(AllEmployeesCacheKey, out List<EmployeeResponseDTO?>? cachedEmployees) && cachedEmployees is not null)
            {
                return cachedEmployees;
            }

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.GetAllEmployees", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            var employees = new List<EmployeeResponseDTO?>();
            try
            {
                await conn.OpenAsync();

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var employee = new EmployeeResponseDTO
                    {
                        employee_id = reader.GetInt32(reader.GetOrdinal("employee_id")),
                        first_name = reader.GetString(reader.GetOrdinal("first_name")),
                        last_name = reader.GetString(reader.GetOrdinal("last_name")),
                        email = reader.GetString(reader.GetOrdinal("email")),
                        phone = reader.GetString(reader.GetOrdinal("phone")),
                        role = reader.GetString(reader.GetOrdinal("role")),
                        is_active = reader.GetBoolean(reader.GetOrdinal("is_active"))
                    };

                    employees.Add(employee);
                    cache.Set(EmployeeByIdCacheKey(employee.employee_id), employee, EmployeeCacheTtl);
                }

                cache.Set(AllEmployeesCacheKey, employees, EmployeeCacheTtl);
                return employees;

            }
            catch (Exception ex)
            {
                Logger.LogError("Error fetching all the employees: ", ex);
                return employees;
            }

        }

        public async Task<bool> DeactivateEmployeeAsync(int employee_id)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.DeleteEmployee ", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@employee_id", SqlDbType.Int).Value = employee_id;

            try
            {
                await conn.OpenAsync();

                
                var rows = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                var wasDeactivated = rows > 0;
                if (wasDeactivated)
                {
                    InvalidateEmployeeCache(employee_id);
                }

                return wasDeactivated;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error deactivating employee with ID {employee_id}", ex);
                return false;

            }

        }

        public async Task<List<EmployeeResponseDTO>?> GetAvailableWorkersPerDay(DateOnly date)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.AvailableWorkersPerDay", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@work_date", SqlDbType.Date).Value = date;

            var employees = new List<EmployeeResponseDTO>();

            try
            {
                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    employees.Add(new EmployeeResponseDTO
                    {
                        employee_id = reader.GetInt32(reader.GetOrdinal("employee_id")),
                        first_name = reader.GetString(reader.GetOrdinal("first_name")),
                        last_name = reader.GetString(reader.GetOrdinal("last_name")),
                        email = reader.GetString(reader.GetOrdinal("email")),
                        phone = reader.GetString(reader.GetOrdinal("phone")),
                        role = reader.GetString(reader.GetOrdinal("role")),
                        is_active = reader.GetBoolean(reader.GetOrdinal("is_active"))
                    });
                }
                return employees;




            }
            catch (Exception ex)
            {
                Logger.LogError("Couldn't retrieve any available emp: ", ex);
                return null;
            }


        }
    }
}
