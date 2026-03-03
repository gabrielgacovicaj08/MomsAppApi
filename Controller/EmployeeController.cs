using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MomsAppApi.Entities;
using MomsAppApi.Models.EmployeeDTO;
using MomsAppApi.Services.EmployeeService;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MomsAppApi.Controller
{



    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {


        [Authorize(Roles = "ADMIN")]
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
        [Authorize(Roles = "ADMIN,WORKER")]
        public async Task<ActionResult<EmployeeResponseDTO?>> GetEmployeeById(int employee_id)
        {
            var isAdmin = User.IsInRole("ADMIN");
            if (!isAdmin)
            {
                var userEmployeeId = User.FindFirstValue("employee_id");
                if (!int.TryParse(userEmployeeId, out var callerEmployeeId) || callerEmployeeId != employee_id)
                {
                    return Forbid();
                }
            }

            var employee = await employeeService.GetEmployeeByIdAsync(employee_id);
            if (employee == null)
            {
                Logger.LogError($"Employee with ID {employee_id} not found.", new Exception("EmployeeNotFound"));
                return NotFound("Employee not found.");
            }
            return Ok(employee);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("update-employee/{employee_id}")]
        [HttpPut("employee/{employee_id}")]
        public async Task<ActionResult<EmployeeResponseDTO?>> UpdateEmployee(int employee_id, UpdateEmployeeRequestDTO updatedEmployee)
        {
            var employee = await employeeService.UpdateEmployeeAsync(employee_id, updatedEmployee);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }
            return Ok(employee);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("all-employees")]
        public async Task<ActionResult<List<EmployeeResponseDTO>>> GetAllEmployees()
        {
            var employees = await employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("employee/{employee_id}/deactivate")]
        public async Task<ActionResult> DeactivateEmployee(int employee_id)
        {
            var success = await employeeService.DeactivateEmployeeAsync(employee_id);
            if (!success)
            {
                return NotFound("Employee not found.");
            }
            return Ok("Employee deactivated successfully.");
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("available-employee-per-day/{date}")]
        public async Task<ActionResult<List<EmployeeResponseDTO?>?>> GetAvailableEmployeesPerDay(DateOnly date)
        {
            var employees = await employeeService.GetAvailableWorkersPerDay(date);
            return employees;


        }
    }
}
