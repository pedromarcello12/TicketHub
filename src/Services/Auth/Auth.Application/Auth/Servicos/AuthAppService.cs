using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using Auth.Domain.Entidades;

namespace Auth.Application.Auth.Servicos;

public class AuthAppService(
    IUsuarioRepositorio repositorio,
    IPasswordHasher passwordHasher) : IAuthAppService
{
    private const string PapelPadraoNovoUsuario = "Cliente";

    public async Task<UsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var existente = await repositorio.ObterPorNomeUsuarioAsync(request.NomeUsuario, cancellationToken);
        if (existente is not null)
            throw new InvalidOperationException($"Nome de usuário '{request.NomeUsuario}' já está em uso.");

        var senhaHash = passwordHasher.Hash(request.Senha);
        var usuario = new Usuario(request.NomeUsuario, senhaHash, request.Nome, PapelPadraoNovoUsuario);

        await repositorio.AdicionarAsync(usuario, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return UsuarioResponse.DeEntidade(usuario);
    }

    public async Task<UsuarioResponse?> AutenticarAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await repositorio.ObterPorNomeUsuarioAsync(request.NomeUsuario, cancellationToken);
        if (usuario is null)
            return null;

        return passwordHasher.Verificar(request.Senha, usuario.SenhaHash)
            ? UsuarioResponse.DeEntidade(usuario)
            : null;
    }
}
