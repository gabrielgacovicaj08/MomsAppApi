using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomsAppApi.Models.AssignmentDTO;
using MomsAppApi.Services.AssignmentService;
using System.Security.Claims;

namespace MomsAppApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssignmentController(IAssignmentService assignmentService) : ControllerBase
    {
        [Authorize(Roles = "ADMIN")]
        [HttpPost("create-assignemnt")]
        public async Task<ActionResult<CreateAssignmentDTO?>> CreateAssignmentAsync(CreateAssignmentDTO request)
        {
            var response = await assignmentService.CreateAssignmentAsync(request);
            if(response == null) return BadRequest("Couldn't create the Assignment.");
            return Ok(response);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("assignments-by-day/{date}")]
        public async Task<ActionResult<ResponseAssignmentDTO?>> GetAllAssignmentByDay(DateOnly date)
        {
            var response = await assignmentService.GetAllAssignmentsByDay(date);
            if (response == null) return BadRequest("Couldn't fetch Assignments");
            if (response.Count == 0) return Ok("No Assignment for today");
            return Ok(response);
        }

        [Authorize(Roles = "ADMIN,WORKER")]
        [HttpGet("assignment-by-empId/{employee_id}")]
        public async Task<ActionResult<List<ResponseAssignmentDTO>?>> GetAssignmentByEmpId(int employee_id)
        {
            var requesterRole = User.FindFirstValue(ClaimTypes.Role);
            var requesterEmployeeIdClaim = User.FindFirst("employee_id")?.Value;

            if (string.Equals(requesterRole, "WORKER", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(requesterEmployeeIdClaim, out var requesterEmployeeId))
                    return Forbid();

                if (requesterEmployeeId != employee_id)
                    return Forbid();
            }

            var response = await assignmentService.GetAssignementsByEmpId(employee_id);
            if (response == null) return BadRequest("Couldn't fetch the assignments");
            if (response.Count == 0) return Ok("No Assignments found for today");
            return Ok(response);
        }
    }
}
