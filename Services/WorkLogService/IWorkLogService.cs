using MomsAppApi.Models.WorkLogDTO;

namespace MomsAppApi.Services.WorkLogService
{
    public interface IWorkLogService
    {
        Task<Boolean> CreateWorkLog(WorkLogRequestDTO request);
    }
}
