using System.ComponentModel.DataAnnotations;

namespace MomsAppApi.Models.EmployeeDTO
{
    public class CreateEmployeeDTO
    {
        [Required]
        [MaxLength(100)]
        public string first_name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string last_name { get; set; } = string.Empty;

        [Phone]
        [MaxLength(30)]
        public string phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string email { get; set; } = string.Empty;

        [Required]
        [RegularExpression("(?i)^(ADMIN|WORKER)$", ErrorMessage = "Role must be ADMIN or WORKER.")]
        public string role { get; set; } = string.Empty;
    }
}
