using TicketHub.Core.Entidades;

namespace Auth.Domain.Entidades;

public class Usuario : EntidadeBase
{
    public string NomeUsuario { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string Papel { get; private set; } = string.Empty;

    private Usuario() { }

    public Usuario(string nomeUsuario, string senhaHash, string nome, string papel)
    {
        if (string.IsNullOrWhiteSpace(nomeUsuario))
            throw new ArgumentException("O nome de usuário é obrigatório.", nameof(nomeUsuario));

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("O hash da senha é obrigatório.", nameof(senhaHash));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome é obrigatório.", nameof(nome));

        if (string.IsNullOrWhiteSpace(papel))
            throw new ArgumentException("O papel é obrigatório.", nameof(papel));

        NomeUsuario = nomeUsuario;
        SenhaHash = senhaHash;
        Nome = nome;
        Papel = papel;
    }

    public void AtualizarSenhaHash(string novaSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaHash))
            throw new ArgumentException("O hash da senha é obrigatório.", nameof(novaSenhaHash));

        SenhaHash = novaSenhaHash;
    }
}
