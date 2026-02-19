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
        [HttpPost("create-assignment")]
        public async Task<ActionResult<CreateAssignmentDTO?>> CreateAssignmentAsync(CreateAssignmentDTO request)
        {
            var response = await assignmentService.CreateAssignmentAsync(request);
            if (response == null) return BadRequest(response);
            return Ok(response);
        }
         
    }
}
