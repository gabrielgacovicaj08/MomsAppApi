using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomsAppApi.Models.WorkLogDTO;
using MomsAppApi.Services.AssignmentService;
using MomsAppApi.Services.WorkLogService;
using System.Security.Claims;

namespace MomsAppApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkLogController(IWorkLogService worklogService, IAssignmentService assignmentService) : ControllerBase
    {
        [Authorize(Roles = "ADMIN,WORKER")]
        [HttpPost("create-worklog")]
        public async Task<ActionResult<Boolean>> CreateWorkLog(WorkLogRequestDTO request)
        {
            if (User.IsInRole("WORKER"))
            {
                var workerEmployeeIdClaim = User.FindFirstValue("employee_id");
                if (!int.TryParse(workerEmployeeIdClaim, out var workerEmployeeId))
                {
                    return Forbid();
                }

                var workerAssignments = await assignmentService.GetAssignementsByEmpId(workerEmployeeId);
                var hasMatchingAssignment = workerAssignments?.Any(a => a?.assignment_id == request.assignment_id) == true;

                if (!hasMatchingAssignment)
                {
                    return Forbid();
                }
            }

            var created = await worklogService.CreateWorkLog(request);
            if (created)
            {
                return Ok("Work log created successfully.");
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create work log",
                Detail = "Check assignment_id, ensure times are valid (not future, ended_at > started_at), and keep shift duration within policy."
            });
        }

    }
}
