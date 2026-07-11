namespace TicketHub.Auth;

public interface IJwtTokenGenerator
{
    string GerarToken(string usuarioId, string nome, string papel);
}
