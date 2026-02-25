namespace MomsAppApi.Models.WorkLogDTO
{
    public class WorkLogResponseDTO
    {
        public DateOnly work_date {  get; set; }
        public DateTime started_at { get; set; }
        public DateTime ended_at { get; set; }
        public string notes { get; set; } = string.Empty;
        public DateTime submitted_at { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string structure_name { get; set; }
    }
}
