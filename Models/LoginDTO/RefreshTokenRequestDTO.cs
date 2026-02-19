using System.Numerics;

namespace MomsAppApi.Models.LoginDTO
{
    public class RefreshTokenRequestDTO
    {
        public int user_id { get; set; }
        public required string refresh_token { get; set; }
    }
}
