namespace MomsAppApi.Models.LoginDTO
{
    public class LoginAttemptResultDTO
    {
        public TokenResponseDTO? Tokens { get; init; }
        public bool IsLockedOut { get; init; }
        public int? RetryAfterSeconds { get; init; }

        public static LoginAttemptResultDTO Success(TokenResponseDTO tokens) => new()
        {
            Tokens = tokens
        };

        public static LoginAttemptResultDTO InvalidCredentials() => new();

        public static LoginAttemptResultDTO LockedOut(int retryAfterSeconds) => new()
        {
            IsLockedOut = true,
            RetryAfterSeconds = retryAfterSeconds
        };
    }
}
