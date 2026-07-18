using Auth.Domain.Entidades;

namespace Auth.Application.Auth.Interfaces;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario, CancellationToken cancellationToken);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
