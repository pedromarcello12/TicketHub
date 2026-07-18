using System.Security.Cryptography;
using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using Auth.Domain.Entidades;
using Microsoft.Extensions.Options;

namespace Auth.Application.Auth.Servicos;

public class AuthAppService(
    IUsuarioRepositorio usuarioRepositorio,
    IRefreshTokenRepositorio refreshTokenRepositorio,
    IPasswordHasher passwordHasher,
    IOptions<RefreshTokenOptions> refreshTokenOpcoes) : IAuthAppService
{
    private const string PapelPadraoNovoUsuario = "Cliente";

    public async Task<ResultadoAutenticacao> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var existente = await usuarioRepositorio.ObterPorNomeUsuarioAsync(request.NomeUsuario, cancellationToken);
        if (existente is not null)
            throw new InvalidOperationException($"Nome de usuário '{request.NomeUsuario}' já está em uso.");

        var senhaHash = passwordHasher.Hash(request.Senha);
        var usuario = new Usuario(request.NomeUsuario, senhaHash, request.Nome, PapelPadraoNovoUsuario);

        await usuarioRepositorio.AdicionarAsync(usuario, cancellationToken);
        await usuarioRepositorio.SalvarAlteracoesAsync(cancellationToken);

        var refreshToken = await CriarRefreshTokenAsync(usuario.Id, cancellationToken);
        await refreshTokenRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return new ResultadoAutenticacao(UsuarioResponse.DeEntidade(usuario), refreshToken);
    }

    public async Task<ResultadoAutenticacao?> AutenticarAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.ObterPorNomeUsuarioAsync(request.NomeUsuario, cancellationToken);
        if (usuario is null || !passwordHasher.Verificar(request.Senha, usuario.SenhaHash))
            return null;

        var refreshToken = await CriarRefreshTokenAsync(usuario.Id, cancellationToken);
        await refreshTokenRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return new ResultadoAutenticacao(UsuarioResponse.DeEntidade(usuario), refreshToken);
    }

    public async Task<ResultadoAutenticacao?> RenovarTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenExistente = await refreshTokenRepositorio.ObterPorTokenAsync(refreshToken, cancellationToken);
        if (tokenExistente is null || !tokenExistente.EstaValido())
            return null;

        var usuario = await usuarioRepositorio.ObterPorIdAsync(tokenExistente.UsuarioId, cancellationToken);
        if (usuario is null)
            return null;

        tokenExistente.Revogar();
        var novoRefreshToken = await CriarRefreshTokenAsync(usuario.Id, cancellationToken);
        await refreshTokenRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return new ResultadoAutenticacao(UsuarioResponse.DeEntidade(usuario), novoRefreshToken);
    }

    private async Task<string> CriarRefreshTokenAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var valor = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiraEm = DateTime.UtcNow.AddDays(refreshTokenOpcoes.Value.ExpiracaoDias);

        var refreshToken = new RefreshToken(valor, usuarioId, expiraEm);
        await refreshTokenRepositorio.AdicionarAsync(refreshToken, cancellationToken);

        return valor;
    }
}
