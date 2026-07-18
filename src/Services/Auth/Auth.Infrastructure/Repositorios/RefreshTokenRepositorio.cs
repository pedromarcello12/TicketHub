using Auth.Application.Auth.Interfaces;
using Auth.Domain.Entidades;
using Auth.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositorios;

public class RefreshTokenRepositorio(AuthDbContext dbContext) : IRefreshTokenRepositorio
{
    public async Task<RefreshToken?> ObterPorTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
