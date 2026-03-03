using System.ComponentModel.DataAnnotations;

namespace MomsAppApi.Models.EmployeeDTO
{
    public class UpdateEmployeeRequestDTO
    {
        [MaxLength(100)]
        public string? first_name { get; set; }

        [MaxLength(100)]
        public string? last_name { get; set; }

        [Phone]
        [MaxLength(30)]
        public string? phone { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? email { get; set; }

        [RegularExpression("(?i)^(ADMIN|WORKER)$", ErrorMessage = "Role must be ADMIN or WORKER.")]
        public string? role { get; set; }

        public bool? is_active { get; set; }
    }
}
