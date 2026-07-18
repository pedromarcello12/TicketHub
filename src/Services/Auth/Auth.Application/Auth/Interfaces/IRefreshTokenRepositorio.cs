using Auth.Domain.Entidades;

namespace Auth.Application.Auth.Interfaces;

public interface IRefreshTokenRepositorio
{
    Task<RefreshToken?> ObterPorTokenAsync(string token, CancellationToken cancellationToken);
    Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
