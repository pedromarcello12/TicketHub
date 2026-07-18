using Auth.Application.Auth.Interfaces;
using Auth.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using TicketHub.Auth;

namespace Auth.Infrastructure.Persistencia;

public static class UsuariosSeeder
{
    public static async Task SemearAsync(
        AuthDbContext dbContext,
        IPasswordHasher passwordHasher,
        ServicoInternoOptions servicoInterno,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Usuarios.AnyAsync(cancellationToken))
            return;

        if (string.IsNullOrWhiteSpace(servicoInterno.Senha))
            throw new InvalidOperationException(
                "ServicoInterno:Senha nao foi configurada. Defina a variavel de ambiente ServicoInterno__Senha (ou user-secrets em desenvolvimento) antes de iniciar o Auth.Api.");

        var admin = new Usuario("admin", passwordHasher.Hash("admin123"), "Administrador TicketHub", Papeis.Administrador);
        var cliente = new Usuario("cliente", passwordHasher.Hash("cliente123"), "Cliente TicketHub", Papeis.Cliente);
        var servico = new Usuario(
            servicoInterno.NomeUsuario,
            passwordHasher.Hash(servicoInterno.Senha),
            "Servico Interno TicketHub",
            Papeis.Servico);

        await dbContext.Usuarios.AddRangeAsync([admin, cliente, servico], cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
