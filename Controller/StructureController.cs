using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomsAppApi.Models.StructuresDTO;
using MomsAppApi.Services.StructureService;

namespace MomsAppApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class StructureController(IStructureService structureService) : ControllerBase
    {
        [HttpPost("create-structure")]
        public async Task<ActionResult<CreateStructureDTO?>> CreateStructureAsync(CreateStructureDTO request)
        {
            var resposne = await structureService.CreateStructureAsync(request);
            if (resposne == null) return BadRequest(resposne);

            return Ok(resposne);
        }

        [HttpPost("update-structure/{id}")]
        public async Task<ActionResult<bool>> UpdateStructure(int id, UpdateStructureDTO request)
        {
            var response = await structureService.UpdateStructureAsync(id, request);
            if (response) return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("all-structures")]
        public async Task<ActionResult<List<StructureResponseDTO>>> GetAllStructuresAsync()
        {
            var response = await structureService.GetAllStructuresAsync();
            if (response != null) return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("structure/{id}")]
        public async Task<ActionResult<StructureResponseDTO?>> GetStructureByIdAsync(int id)
        {
            var response = await structureService.GetStructureByIdAsync(id);
            if (response != null) return Ok(response);
            return NotFound(response);
        }

        [HttpGet("delete-structure/{id}")]
        public async Task<ActionResult<bool>> DeleteStructureAsync(int id)
        {
            var response = await structureService.DeleteStructureAsync(id);
            if (response) return Ok(response);
            return BadRequest(response);
        }
    }
}
