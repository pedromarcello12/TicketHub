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
        if (string.IsNullOrWhiteSpace(servicoInterno.Senha))
            throw new InvalidOperationException(
                "ServicoInterno:Senha nao foi configurada. Defina a variavel de ambiente ServicoInterno__Senha antes de iniciar o Auth.Api.");

        // Garante que o usuário de serviço interno sempre exista (necessário para comunicação entre serviços)
        var jaExiste = await dbContext.Usuarios
            .AnyAsync(u => u.NomeUsuario == servicoInterno.NomeUsuario, cancellationToken);

        if (jaExiste)
            return;

        var servico = new Usuario(
            servicoInterno.NomeUsuario,
            passwordHasher.Hash(servicoInterno.Senha),
            "Servico Interno",
            Papeis.Servico);

        await dbContext.Usuarios.AddAsync(servico, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
