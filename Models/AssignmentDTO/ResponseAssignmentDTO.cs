using System.Globalization;

namespace MomsAppApi.Models.AssignmentDTO
{
    public class ResponseAssignmentDTO
    {
        public int assignment_id { get; set; }
        public DateOnly work_date { get; set; }
        public TimeOnly? shift_start { get; set; }
        public TimeOnly? shift_end { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string structure_name { get; set; }
        public string status { get; set; }
    }
}
