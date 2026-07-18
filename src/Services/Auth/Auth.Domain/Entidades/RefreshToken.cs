using TicketHub.Core.Entidades;

namespace Auth.Domain.Entidades;

public class RefreshToken : EntidadeBase
{
    public string Token { get; private set; } = string.Empty;
    public Guid UsuarioId { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public DateTime? RevogadoEm { get; private set; }

    private RefreshToken() { }

    public RefreshToken(string token, Guid usuarioId, DateTime expiraEm)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("O token é obrigatório.", nameof(token));

        if (usuarioId == Guid.Empty)
            throw new ArgumentException("O refresh token precisa estar vinculado a um usuário.", nameof(usuarioId));

        if (expiraEm <= DateTime.UtcNow)
            throw new ArgumentException("A expiração precisa ser no futuro.", nameof(expiraEm));

        Token = token;
        UsuarioId = usuarioId;
        ExpiraEm = expiraEm;
    }

    public bool EstaValido() => RevogadoEm is null && DateTime.UtcNow < ExpiraEm;

    public void Revogar()
    {
        RevogadoEm ??= DateTime.UtcNow;
    }
}
