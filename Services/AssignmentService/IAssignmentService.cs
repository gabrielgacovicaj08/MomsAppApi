using MomsAppApi.Models.AssignmentDTO;

namespace MomsAppApi.Services.AssignmentService
{
    public interface IAssignmentService
    {
        Task<CreateAssignmentDTO> CreateAssignmentAsync(CreateAssignmentDTO request);
        Task<bool> UpdateAssignmentAsync(int assignment_id, UpdateAssignmentDTO request);
        Task<List<ResponseAssignmentDTO>> GetAllAssignmentsByDay(DateOnly date);

        Task<List<ResponseAssignmentDTO>> GetAssignementsByEmpId(int employee_id);
    }
}
