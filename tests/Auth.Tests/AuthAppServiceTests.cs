using Auth.Application.Auth;
using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using Auth.Application.Auth.Servicos;
using Auth.Domain.Entidades;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Auth.Tests;

public class AuthAppServiceTests
{
    private readonly IUsuarioRepositorio _usuarioRepositorio = Substitute.For<IUsuarioRepositorio>();
    private readonly IRefreshTokenRepositorio _refreshTokenRepositorio = Substitute.For<IRefreshTokenRepositorio>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly AuthAppService _sut;

    public AuthAppServiceTests()
    {
        var refreshTokenOpcoes = Options.Create(new RefreshTokenOptions { ExpiracaoDias = 7 });
        _sut = new AuthAppService(_usuarioRepositorio, _refreshTokenRepositorio, _passwordHasher, refreshTokenOpcoes);
    }

    [Fact]
    public async Task RegistrarAsync_ComNomeUsuarioDisponivel_DeveCriarComPapelCliente()
    {
        _usuarioRepositorio.ObterPorNomeUsuarioAsync("novo", Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _passwordHasher.Hash("senha123").Returns("hash-fake");

        var resultado = await _sut.RegistrarAsync(new RegistrarUsuarioRequest("novo", "senha123", "Novo Usuario"), CancellationToken.None);

        resultado.Usuario.NomeUsuario.Should().Be("novo");
        resultado.Usuario.Papel.Should().Be("Cliente");
        resultado.RefreshToken.Should().NotBeNullOrWhiteSpace();
        await _usuarioRepositorio.Received(1).AdicionarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        await _usuarioRepositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await _refreshTokenRepositorio.Received(1).AdicionarAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegistrarAsync_ComNomeUsuarioJaExistente_DeveLancarExcecaoENaoSalvar()
    {
        var existente = new Usuario("admin", "hash", "Administrador", "Administrador");
        _usuarioRepositorio.ObterPorNomeUsuarioAsync("admin", Arg.Any<CancellationToken>()).Returns(existente);

        var acao = async () => await _sut.RegistrarAsync(new RegistrarUsuarioRequest("admin", "senha123", "Outro"), CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
        await _usuarioRepositorio.DidNotReceive().AdicionarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AutenticarAsync_ComUsuarioInexistente_DeveRetornarNulo()
    {
        _usuarioRepositorio.ObterPorNomeUsuarioAsync("fantasma", Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await _sut.AutenticarAsync(new LoginRequest("fantasma", "qualquer"), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AutenticarAsync_ComSenhaInvalida_DeveRetornarNulo()
    {
        var usuario = new Usuario("admin", "hash-armazenado", "Administrador", "Administrador");
        _usuarioRepositorio.ObterPorNomeUsuarioAsync("admin", Arg.Any<CancellationToken>()).Returns(usuario);
        _passwordHasher.Verificar("senha-errada", "hash-armazenado").Returns(false);

        var resultado = await _sut.AutenticarAsync(new LoginRequest("admin", "senha-errada"), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AutenticarAsync_ComCredenciaisValidas_DeveRetornarUsuarioERefreshToken()
    {
        var usuario = new Usuario("admin", "hash-armazenado", "Administrador TicketHub", "Administrador");
        _usuarioRepositorio.ObterPorNomeUsuarioAsync("admin", Arg.Any<CancellationToken>()).Returns(usuario);
        _passwordHasher.Verificar("admin123", "hash-armazenado").Returns(true);

        var resultado = await _sut.AutenticarAsync(new LoginRequest("admin", "admin123"), CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Usuario.Papel.Should().Be("Administrador");
        resultado.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RenovarTokenAsync_ComTokenValido_DeveRevogarOAntigoERetornarNovoToken()
    {
        var usuario = new Usuario("admin", "hash", "Administrador TicketHub", "Administrador");
        var tokenAntigo = new RefreshToken("token-antigo", usuario.Id, DateTime.UtcNow.AddDays(1));
        _refreshTokenRepositorio.ObterPorTokenAsync("token-antigo", Arg.Any<CancellationToken>()).Returns(tokenAntigo);
        _usuarioRepositorio.ObterPorIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns(usuario);

        var resultado = await _sut.RenovarTokenAsync("token-antigo", CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.RefreshToken.Should().NotBe("token-antigo");
        tokenAntigo.EstaValido().Should().BeFalse();
        await _refreshTokenRepositorio.Received(1).AdicionarAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RenovarTokenAsync_ComTokenInexistente_DeveRetornarNulo()
    {
        _refreshTokenRepositorio.ObterPorTokenAsync("nao-existe", Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        var resultado = await _sut.RenovarTokenAsync("nao-existe", CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task RenovarTokenAsync_ComTokenExpirado_DeveRetornarNulo()
    {
        var usuario = new Usuario("admin", "hash", "Administrador TicketHub", "Administrador");
        var tokenExpirado = new RefreshToken("token-expirado", usuario.Id, DateTime.UtcNow.AddSeconds(1));
        await Task.Delay(1100);
        _refreshTokenRepositorio.ObterPorTokenAsync("token-expirado", Arg.Any<CancellationToken>()).Returns(tokenExpirado);

        var resultado = await _sut.RenovarTokenAsync("token-expirado", CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task RenovarTokenAsync_ComTokenJaRevogado_DeveRetornarNulo()
    {
        var usuario = new Usuario("admin", "hash", "Administrador TicketHub", "Administrador");
        var tokenRevogado = new RefreshToken("token-revogado", usuario.Id, DateTime.UtcNow.AddDays(1));
        tokenRevogado.Revogar();
        _refreshTokenRepositorio.ObterPorTokenAsync("token-revogado", Arg.Any<CancellationToken>()).Returns(tokenRevogado);

        var resultado = await _sut.RenovarTokenAsync("token-revogado", CancellationToken.None);

        resultado.Should().BeNull();
    }
}
