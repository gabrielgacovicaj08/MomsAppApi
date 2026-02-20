using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MomsAppApi.Entities;
using MomsAppApi.Models.EmployeeDTO;
using MomsAppApi.Services.EmployeeService;
using System.Threading.Tasks;

namespace MomsAppApi.Controller
{



    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {


        //[Authorize(Roles = "ADMIN")]
        [HttpPost("create-employee")]
        public async Task<ActionResult<EmployeeResponseDTO?>> CreateEmployee(CreateEmployeeDTO request)
        {
            var employee = await employeeService.CreateEmployeeAsync(request);
            if (employee == null)
            {
                return BadRequest("Failed to create employee.");
            }

            return Ok(employee);
        }

        [HttpGet("employee/{employee_id}")]
        public async Task<ActionResult<EmployeeResponseDTO?>> GetEmployeeById(int employee_id)
        {

            var employee = await employeeService.GetEmployeeByIdAsync(employee_id);
            if (employee == null)
            {
                Logger.LogError($"Employee with ID {employee_id} not found.", new Exception("EmployeeNotFound"));
                return NotFound("Employee not found.");
            }
            return Ok(employee);


        }

        [HttpPost("update-employee/{employee_id}")]
        public async Task<ActionResult<Employee?>> UpdateEmployee(int employee_id, Employee updatedEmployee)
        {
            var employee = await employeeService.UpdateEmployeeAsync(employee_id, updatedEmployee);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }
            return Ok(employee);
        }

        [HttpGet("all-employees")]
        public async Task<ActionResult<List<EmployeeResponseDTO>>> GetAllEmployees()
        {
            var employees = await employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("employee/{employee_id}/deactivate")]
        public async Task<ActionResult> DeactivateEmployee(int employee_id)
        {
            var success = await employeeService.DeactivateEmployeeAsync(employee_id);
            if (!success)
            {
                return NotFound("Employee not found.");
            }
            return Ok("Employee deactivated successfully.");
        }
    }
}
