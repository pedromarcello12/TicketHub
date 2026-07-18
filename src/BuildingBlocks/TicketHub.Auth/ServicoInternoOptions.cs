namespace TicketHub.Auth;

public class ServicoInternoOptions
{
    public const string SectionName = "ServicoInterno";

    public string NomeUsuario { get; set; } = "servico-interno";
    public string Senha { get; set; } = string.Empty;
    public string AuthApiBaseUrl { get; set; } = string.Empty;
}
