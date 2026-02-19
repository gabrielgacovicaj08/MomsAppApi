using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MomsAppApi.Data;
using MomsAppApi.Entities;
using MomsAppApi.Models.EmployeeDTO;
using System.Data;

namespace MomsAppApi.Services.EmployeeService
{
    public class EmployeeService(IConfiguration configuration) : IEmployeeService
    {

        private SqlConnection NewConn() => new SqlConnection(configuration.GetConnectionString("MomsAppDb"));

        public async Task<EmployeeResponseDTO?> CreateEmployeeAsync(CreateEmployeeDTO request)
        {
            int new_employee_id;

            await using var conn = NewConn();
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();


            try
            {



                await using (var cmd = new SqlCommand("dbo.CreateEmployee", conn, (SqlTransaction)transaction) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@first_name", request.first_name);
                    cmd.Parameters.AddWithValue("@last_name", request.last_name);
                    cmd.Parameters.AddWithValue("@phone", request.phone);
                    cmd.Parameters.AddWithValue("@email", request.email);
                    cmd.Parameters.AddWithValue("@role", request.role);

                    object? result = await cmd.ExecuteScalarAsync();

                    if (result is null) return null;

                    new_employee_id = Convert.ToInt32(result);
                }



                //context.Employees.Add(employee);
                //await context.SaveChangesAsync(); // generates EmployeeId



                var userAccount = new UserAccount();

                var hashedPassword = new PasswordHasher<UserAccount>()
                    .HashPassword(userAccount, request.last_name);


                await using (var cmd = new SqlCommand("dbo.CreateUserAccount", conn, (SqlTransaction)transaction) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@employee_id", new_employee_id);
                    cmd.Parameters.AddWithValue("@email", request.email);
                    cmd.Parameters.AddWithValue("@password_hash", hashedPassword);
                    cmd.Parameters.AddWithValue("@role", request.role);
                    await cmd.ExecuteNonQueryAsync();
                }



                await transaction.CommitAsync();
                var response = new EmployeeResponseDTO
                {
                    first_name = request.first_name,
                    last_name = request.last_name,
                    email = request.email,
                    phone = request.phone,
                    role = request.role,
                    is_active = true
                };


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

                // Map the row to your DTO
                return new EmployeeResponseDTO
                {
                    employee_id = employee_id,
                    first_name = reader.GetString(reader.GetOrdinal("first_name")),
                    last_name = reader.GetString(reader.GetOrdinal("last_name")),
                    email = reader.GetString(reader.GetOrdinal("email")),
                    phone = reader.GetString(reader.GetOrdinal("phone")),
                    role = reader.GetString(reader.GetOrdinal("role")),
                    is_active = reader.GetBoolean(reader.GetOrdinal("is_active"))
                };
            }
            catch (Exception ex)
            {

                Logger.LogError($"Error fetching employee with ID {employee_id}", ex);

                return null;
            }

        }

        public async Task<Employee?> UpdateEmployeeAsync(int employee_id, Employee updatedEmployee)
        {

            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.UpdateEmployee", conn)
            {
                CommandType = CommandType.StoredProcedure
            };



            cmd.Parameters.AddWithValue("@employee_id", employee_id);

            cmd.Parameters.Add("@first_name", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(updatedEmployee.first_name) ? DBNull.Value : updatedEmployee.first_name.Trim();
            cmd.Parameters.Add("@last_name", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(updatedEmployee.last_name) ? DBNull.Value : updatedEmployee.last_name.Trim();
            cmd.Parameters.Add("@phone", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(updatedEmployee.phone) ? DBNull.Value : updatedEmployee.phone.Trim();
            cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(updatedEmployee.email) ? DBNull.Value : updatedEmployee.email.Trim();
            cmd.Parameters.Add("@role", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(updatedEmployee.role) ? DBNull.Value : updatedEmployee.role.Trim();
            cmd.Parameters.Add("@is_active", SqlDbType.NVarChar, 100).Value = (object?)updatedEmployee.is_active ?? DBNull.Value;

            try
            {
                await conn.OpenAsync();
                
                var rows = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                if (rows == 0) return null;

                return updatedEmployee;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error updating employee with ID {employee_id}", ex);
                return null;



            }
        }

        public async Task<List<EmployeeResponseDTO?>> GetAllEmployeesAsync()
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.GetAllEmployees", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

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
                Logger.LogError("Error fetching all the employees: ", ex);
                return employees;
            }

        }

        public async Task<bool> DeactivateEmployeeAsync(int employee_id)
        {
            await using var conn = NewConn();
            await using var cmd = new SqlCommand("dbo.DeactivateEmployee ", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@employee_id", SqlDbType.Int).Value = employee_id;

            try
            {
                await conn.OpenAsync();

               
                var rows = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return rows > 0;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error deactivating employee with ID {employee_id}", ex);
                return false;

            }

        }
    }
}
