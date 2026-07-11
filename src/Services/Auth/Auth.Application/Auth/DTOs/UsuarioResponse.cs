using Auth.Domain.Entidades;

namespace Auth.Application.Auth.DTOs;

public record UsuarioResponse(Guid Id, string NomeUsuario, string Nome, string Papel)
{
    public static UsuarioResponse DeEntidade(Usuario usuario) => new(
        usuario.Id,
        usuario.NomeUsuario,
        usuario.Nome,
        usuario.Papel);
}
