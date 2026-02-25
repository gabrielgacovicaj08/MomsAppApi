using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomsAppApi.Models.AssignmentDTO;
using MomsAppApi.Services.AssignmentService;

namespace MomsAppApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController(IAssignmentService assignmentService) : ControllerBase
    {
        [HttpPost("create-assignemnt")]
        public async Task<ActionResult<CreateAssignmentDTO?>> CreateAssignmentAsync(CreateAssignmentDTO request)
        {
            var response = await assignmentService.CreateAssignmentAsync(request);
            if(response == null) return BadRequest("Couldn't create the Assignment.");
            return Ok(response);
        }

        [HttpGet("assignments-by-day/{date}")]
        public async Task<ActionResult<ResponseAssignmentDTO?>> GetAllAssignmentByDay(DateOnly date)
        {
            var response = new List<ResponseAssignmentDTO>();
            response = await assignmentService.GetAllAssignmentsByDay(date);
            if (response.Count == 0) return Ok("No Assignment for today");
            else if (response == null) return BadRequest("Couldn't fetch Assignments");
            return Ok(response);
        }

        [HttpGet("assignment-by-empId/{employee_id}")]
        public async Task<ActionResult<List<ResponseAssignmentDTO>?>> GetAssignmentByEmpId(int employee_id)
        {
            var response = new List<ResponseAssignmentDTO>();
            response = await assignmentService.GetAssignementsByEmpId(employee_id);
            if (response.Count == 0) return Ok("No Assignments found for today");
            else if (response == null) return BadRequest("Couldn't fetch the assignments");
            return Ok(response);
        }
    }
}
