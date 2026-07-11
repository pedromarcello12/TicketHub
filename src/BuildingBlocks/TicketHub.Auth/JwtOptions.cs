namespace TicketHub.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "TicketHub";
    public string Audience { get; set; } = "TicketHub";
    public int ExpiracaoMinutos { get; set; } = 60;
}
