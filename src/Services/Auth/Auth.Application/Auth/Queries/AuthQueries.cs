using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using MediatR;

namespace Auth.Application.Auth.Queries;

// ─── ObterPerfil ─────────────────────────────────────────────────────────────

public record ObterPerfilQuery(Guid UsuarioId) : IRequest<UsuarioResponse?>;

public class ObterPerfilQueryHandler(IUsuarioRepositorio usuarioRepositorio)
    : IRequestHandler<ObterPerfilQuery, UsuarioResponse?>
{
    public async Task<UsuarioResponse?> Handle(ObterPerfilQuery request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        return usuario is null ? null : UsuarioResponse.DeEntidade(usuario);
    }
}

// ─── ListarUsuarios ───────────────────────────────────────────────────────────

public record ListarUsuariosQuery : IRequest<IReadOnlyList<UsuarioResponse>>;

public class ListarUsuariosQueryHandler(IUsuarioRepositorio usuarioRepositorio)
    : IRequestHandler<ListarUsuariosQuery, IReadOnlyList<UsuarioResponse>>
{
    public async Task<IReadOnlyList<UsuarioResponse>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
    {
        var usuarios = await usuarioRepositorio.ListarAsync(cancellationToken);
        return usuarios.Select(UsuarioResponse.DeEntidade).ToList();
    }
}
