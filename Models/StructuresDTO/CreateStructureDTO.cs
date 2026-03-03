using System.ComponentModel.DataAnnotations;

namespace MomsAppApi.Models.StructuresDTO
{
    public class CreateStructureDTO
    {
        [Required]
        [MaxLength(150)]
        public string name { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string address_line { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string city { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string zip { get; set; } = string.Empty;

        [MaxLength(150)]
        public string client_name { get; set; } = string.Empty;
    }
}
