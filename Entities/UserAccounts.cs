using System.Globalization;

namespace MomsAppApi.Entities
{
    public class UserAccount
    {
        public int user_id { get; set; }
        public int employee_id { get; set; }
        public string email { get; set; } = string.Empty;
        public string password_hash { get; set; } = string.Empty;

        public string role { get; set; } = string.Empty;
        public string? refresh_token { get; set; } = string.Empty;
        public DateTime? refresh_token_expiry_time { get; set; }


    }
}
