using System.Security.Cryptography;
using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using Auth.Domain.Entidades;
using MediatR;

namespace Auth.Application.Auth.Commands;

// ─── Registrar ───────────────────────────────────────────────────────────────

public record RegistrarUsuarioCommand(string NomeUsuario, string Senha, string Nome, string Papel)
    : IRequest<ResultadoAutenticacao>;

public class RegistrarUsuarioCommandHandler(
    IUsuarioRepositorio usuarioRepositorio,
    IRefreshTokenRepositorio refreshTokenRepositorio,
    IPasswordHasher passwordHasher,
    Microsoft.Extensions.Options.IOptions<RefreshTokenOptions> refreshTokenOpcoes)
    : IRequestHandler<RegistrarUsuarioCommand, ResultadoAutenticacao>
{
    private static readonly HashSet<string> PapeisValidos = ["Cliente", "Administrador"];

    public async Task<ResultadoAutenticacao> Handle(RegistrarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var existente = await usuarioRepositorio.ObterPorNomeUsuarioAsync(request.NomeUsuario, cancellationToken);
        if (existente is not null)
            throw new InvalidOperationException($"Nome de usuário '{request.NomeUsuario}' já está em uso.");

        var papel = PapeisValidos.Contains(request.Papel) ? request.Papel : "Cliente";
        var senhaHash = passwordHasher.Hash(request.Senha);
        var usuario = new Usuario(request.NomeUsuario, senhaHash, request.Nome, papel);

        await usuarioRepositorio.AdicionarAsync(usuario, cancellationToken);
        await usuarioRepositorio.SalvarAlteracoesAsync(cancellationToken);

        var refreshToken = await CriarRefreshTokenAsync(usuario.Id, cancellationToken);
        await refreshTokenRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return new ResultadoAutenticacao(UsuarioResponse.DeEntidade(usuario), refreshToken);
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

// ─── Login ───────────────────────────────────────────────────────────────────

public record LoginCommand(string NomeUsuario, string Senha) : IRequest<ResultadoAutenticacao?>;

public class LoginCommandHandler(
    IUsuarioRepositorio usuarioRepositorio,
    IRefreshTokenRepositorio refreshTokenRepositorio,
    IPasswordHasher passwordHasher,
    Microsoft.Extensions.Options.IOptions<RefreshTokenOptions> refreshTokenOpcoes)
    : IRequestHandler<LoginCommand, ResultadoAutenticacao?>
{
    public async Task<ResultadoAutenticacao?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.ObterPorNomeUsuarioAsync(request.NomeUsuario, cancellationToken);
        if (usuario is null || !passwordHasher.Verificar(request.Senha, usuario.SenhaHash))
            return null;

        var valor = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiraEm = DateTime.UtcNow.AddDays(refreshTokenOpcoes.Value.ExpiracaoDias);
        var refreshToken = new RefreshToken(valor, usuario.Id, expiraEm);

        await refreshTokenRepositorio.AdicionarAsync(refreshToken, cancellationToken);
        await refreshTokenRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return new ResultadoAutenticacao(UsuarioResponse.DeEntidade(usuario), valor);
    }
}

// ─── RenovarToken ────────────────────────────────────────────────────────────

public record RenovarTokenCommand(string RefreshToken) : IRequest<ResultadoAutenticacao?>;

public class RenovarTokenCommandHandler(
    IUsuarioRepositorio usuarioRepositorio,
    IRefreshTokenRepositorio refreshTokenRepositorio,
    Microsoft.Extensions.Options.IOptions<RefreshTokenOptions> refreshTokenOpcoes)
    : IRequestHandler<RenovarTokenCommand, ResultadoAutenticacao?>
{
    public async Task<ResultadoAutenticacao?> Handle(RenovarTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenExistente = await refreshTokenRepositorio.ObterPorTokenAsync(request.RefreshToken, cancellationToken);
        if (tokenExistente is null || !tokenExistente.EstaValido())
            return null;

        var usuario = await usuarioRepositorio.ObterPorIdAsync(tokenExistente.UsuarioId, cancellationToken);
        if (usuario is null) return null;

        tokenExistente.Revogar();

        var valor = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        var expiraEm = DateTime.UtcNow.AddDays(refreshTokenOpcoes.Value.ExpiracaoDias);
        var novoRefreshToken = new RefreshToken(valor, usuario.Id, expiraEm);

        await refreshTokenRepositorio.AdicionarAsync(novoRefreshToken, cancellationToken);
        await refreshTokenRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return new ResultadoAutenticacao(UsuarioResponse.DeEntidade(usuario), valor);
    }
}

// ─── AtualizarPerfil ─────────────────────────────────────────────────────────

public record AtualizarPerfilCommand(Guid UsuarioId, string? Nome, string? SenhaAtual, string? NovaSenha)
    : IRequest<UsuarioResponse?>;

public class AtualizarPerfilCommandHandler(
    IUsuarioRepositorio usuarioRepositorio,
    IPasswordHasher passwordHasher)
    : IRequestHandler<AtualizarPerfilCommand, UsuarioResponse?>
{
    public async Task<UsuarioResponse?> Handle(AtualizarPerfilCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Nome))
            usuario.AtualizarNome(request.Nome);

        if (!string.IsNullOrWhiteSpace(request.SenhaAtual) && !string.IsNullOrWhiteSpace(request.NovaSenha))
        {
            if (!passwordHasher.Verificar(request.SenhaAtual, usuario.SenhaHash))
                throw new InvalidOperationException("Senha atual incorreta.");

            usuario.AtualizarSenhaHash(passwordHasher.Hash(request.NovaSenha));
        }

        await usuarioRepositorio.SalvarAlteracoesAsync(cancellationToken);
        return UsuarioResponse.DeEntidade(usuario);
    }
}

// ─── ExcluirUsuario ───────────────────────────────────────────────────────────

public record ExcluirUsuarioCommand(Guid Id) : IRequest<bool>;

public class ExcluirUsuarioCommandHandler(IUsuarioRepositorio usuarioRepositorio)
    : IRequestHandler<ExcluirUsuarioCommand, bool>
{
    public async Task<bool> Handle(ExcluirUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (usuario is null) return false;

        await usuarioRepositorio.RemoverAsync(usuario, cancellationToken);
        await usuarioRepositorio.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}
