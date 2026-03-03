using System.ComponentModel.DataAnnotations;

namespace MomsAppApi.Models.LoginDTO
{
    public class RefreshTokenRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int user_id { get; set; }

        [Required]
        [MinLength(20)]
        public string refresh_token { get; set; } = string.Empty;
    }
}
