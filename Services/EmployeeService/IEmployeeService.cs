using MomsAppApi.Entities;
using MomsAppApi.Models.EmployeeDTO;

namespace MomsAppApi.Services.EmployeeService
{
    public interface IEmployeeService

    {
        Task<EmployeeResponseDTO> CreateEmployeeAsync(CreateEmployeeDTO employee);
        Task<EmployeeResponseDTO?> GetEmployeeByIdAsync(int employee_id);

        Task<Employee ?> UpdateEmployeeAsync(int employee_id, Employee updatedEmployee);
        Task<List<EmployeeResponseDTO?>> GetAllEmployeesAsync();

        Task<bool> DeactivateEmployeeAsync(int employee_id);

    }
}
