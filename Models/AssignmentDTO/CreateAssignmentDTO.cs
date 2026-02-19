using System.Globalization;

namespace MomsAppApi.Models.AssignmentDTO
{
    public class CreateAssignmentDTO
    {
        public DateOnly work_date { get; set; }
        public int employee_id { get; set; }
        public int structure_id { get; set; }
        public TimeOnly? shift_start { get; set; } 
        public TimeOnly? shift_end { get; set; }
    }
}
