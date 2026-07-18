namespace Notificacoes.Worker.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UsarSsl { get; set; }
    public string Remetente { get; set; } = "notificacoes@tickethub.local";
}
