namespace MomsAppApi.Models.EmployeeDTO
{
    public class CreateEmployeeDTO
    {
        public string first_name { get; set; } 
        public string last_name { get; set; }
        public string phone { get; set; } = string.Empty;
        public string email { get; set; }
        public string role { get; set; } 
    }
}
