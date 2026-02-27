using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomsAppApi.Models.WorkLogDTO;
using MomsAppApi.Services.WorkLogService;

namespace MomsAppApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkLogController(IWorkLogService worklogService) : ControllerBase
    {
        [Authorize(Roles = "ADMIN,WORKER")]
        [HttpPost("create-worklog")]
        public async Task<ActionResult<Boolean>> CreateWorkLog(WorkLogRequestDTO request)
        {
            var response = await worklogService.CreateWorkLog(request);
            if ( response) return Ok("WorkLog Created succescfully");
            return BadRequest(response);
        }

    }
}
