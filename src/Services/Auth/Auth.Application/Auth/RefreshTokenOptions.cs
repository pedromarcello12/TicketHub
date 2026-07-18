namespace Auth.Application.Auth;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int ExpiracaoDias { get; set; } = 7;
}
