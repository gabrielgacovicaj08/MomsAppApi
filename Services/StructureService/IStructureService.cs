using MomsAppApi.Models.StructuresDTO;

namespace MomsAppApi.Services.StructureService
{
    public interface IStructureService
    {
        Task<CreateStructureDTO> CreateStructureAsync(CreateStructureDTO request);
        Task<bool> UpdateStructureAsync(int structure_id, UpdateStructureDTO request);
        Task<List<StructureResponseDTO>> GetAllStructuresAsync();
         Task<StructureResponseDTO?> GetStructureByIdAsync(int structure_id);
    }
}
