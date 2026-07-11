using Auth.Application.Auth.Interfaces;
using Auth.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistencia;

public static class UsuariosSeeder
{
    public static async Task SemearAsync(AuthDbContext dbContext, IPasswordHasher passwordHasher, CancellationToken cancellationToken)
    {
        if (await dbContext.Usuarios.AnyAsync(cancellationToken))
            return;

        var admin = new Usuario("admin", passwordHasher.Hash("admin123"), "Administrador TicketHub", "Administrador");
        var cliente = new Usuario("cliente", passwordHasher.Hash("cliente123"), "Cliente TicketHub", "Cliente");

        await dbContext.Usuarios.AddRangeAsync([admin, cliente], cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
