namespace TicketHub.Auth;

public interface IJwtTokenGenerator
{
    string GerarToken(string usuarioId, string nomeUsuario, string nome, string papel);
}
