using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using Auth.Application.Auth.Servicos;
using Auth.Domain.Entidades;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Auth.Tests;

public class AuthAppServiceTests
{
    private readonly IUsuarioRepositorio _repositorio = Substitute.For<IUsuarioRepositorio>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly AuthAppService _sut;

    public AuthAppServiceTests()
    {
        _sut = new AuthAppService(_repositorio, _passwordHasher);
    }

    [Fact]
    public async Task RegistrarAsync_ComNomeUsuarioDisponivel_DeveCriarComPapelCliente()
    {
        _repositorio.ObterPorNomeUsuarioAsync("novo", Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _passwordHasher.Hash("senha123").Returns("hash-fake");

        var resultado = await _sut.RegistrarAsync(new RegistrarUsuarioRequest("novo", "senha123", "Novo Usuario"), CancellationToken.None);

        resultado.NomeUsuario.Should().Be("novo");
        resultado.Papel.Should().Be("Cliente");
        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegistrarAsync_ComNomeUsuarioJaExistente_DeveLancarExcecaoENaoSalvar()
    {
        var existente = new Usuario("admin", "hash", "Administrador", "Administrador");
        _repositorio.ObterPorNomeUsuarioAsync("admin", Arg.Any<CancellationToken>()).Returns(existente);

        var acao = async () => await _sut.RegistrarAsync(new RegistrarUsuarioRequest("admin", "senha123", "Outro"), CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AutenticarAsync_ComUsuarioInexistente_DeveRetornarNulo()
    {
        _repositorio.ObterPorNomeUsuarioAsync("fantasma", Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await _sut.AutenticarAsync(new LoginRequest("fantasma", "qualquer"), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AutenticarAsync_ComSenhaInvalida_DeveRetornarNulo()
    {
        var usuario = new Usuario("admin", "hash-armazenado", "Administrador", "Administrador");
        _repositorio.ObterPorNomeUsuarioAsync("admin", Arg.Any<CancellationToken>()).Returns(usuario);
        _passwordHasher.Verificar("senha-errada", "hash-armazenado").Returns(false);

        var resultado = await _sut.AutenticarAsync(new LoginRequest("admin", "senha-errada"), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AutenticarAsync_ComCredenciaisValidas_DeveRetornarUsuario()
    {
        var usuario = new Usuario("admin", "hash-armazenado", "Administrador TicketHub", "Administrador");
        _repositorio.ObterPorNomeUsuarioAsync("admin", Arg.Any<CancellationToken>()).Returns(usuario);
        _passwordHasher.Verificar("admin123", "hash-armazenado").Returns(true);

        var resultado = await _sut.AutenticarAsync(new LoginRequest("admin", "admin123"), CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Papel.Should().Be("Administrador");
    }
}
