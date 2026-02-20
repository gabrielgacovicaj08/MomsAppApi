using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomsAppApi.Models.AssignmentDTO;
using MomsAppApi.Services.AssignmentService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MomsAppApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController(IAssignmentService assignmentService) : ControllerBase
    {
        [HttpPost("create-assignment")]
        public async Task<ActionResult<CreateAssignmentDTO?>> CreateAssignmentAsync(CreateAssignmentDTO request)
        {
            var response = await assignmentService.CreateAssignmentAsync(request);
            if (response == null) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("assignments-by-day/{date}")]
        public async Task<ActionResult<List<ResponseAssignmentDTO?>>> GetAllAssignmentsByDay(DateOnly date)
        {
            var response = await assignmentService.GetAllAssignmentsByDay(date);
            if (response != null) return response;

            return BadRequest(response);
        }

        [HttpGet("assignements-by-emp-id/{employee_id}")]
        public async Task<ActionResult<List<ResponseAssignmentDTO?>>> GetAssignementsByEmpId(int employee_id)
        {
            var response = await assignmentService.GetAssignementsByEmpId(employee_id);
            if (response != null) return response;

            return BadRequest(response);
        }
    }


}
