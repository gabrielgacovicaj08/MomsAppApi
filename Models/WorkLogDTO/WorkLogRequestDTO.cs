using System.Globalization;

namespace MomsAppApi.Models.WorkLogDTO
{
    public class WorkLogRequestDTO
    {
        public string assignment_id { get; set; }
        public DateTime started_at { get; set; }
        public DateTime ended_at { get; set; }
        public string notes { get; set; } = string.Empty;
    }
}
