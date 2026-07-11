using Auth.Application.Auth.Interfaces;
using Auth.Domain.Entidades;
using Auth.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositorios;

public class UsuarioRepositorio(AuthDbContext dbContext) : IUsuarioRepositorio
{
    public async Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario, CancellationToken cancellationToken)
    {
        return await dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.NomeUsuario.ToLower() == nomeUsuario.ToLower(), cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        await dbContext.Usuarios.AddAsync(usuario, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
