namespace MomsAppApi.Models.EmployeeDTO
{
    public class UpdateEmployeeRequestDTO
    {
        public int employee_id { get; set; }
        public string? first_name { get; set; } = string.Empty;

        public string? last_name { get; set; } = string.Empty;
        public string? phone { get; set; } = string.Empty;
        public string? email { get; set; } = string.Empty;
        public string? role { get; set; } = string.Empty;
        public bool? is_active { get; set; } = true;
    }
}
