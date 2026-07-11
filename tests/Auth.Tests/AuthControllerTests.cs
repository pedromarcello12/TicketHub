using Auth.Api.Controllers;
using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using TicketHub.Auth;
using Xunit;

namespace Auth.Tests;

public class AuthControllerTests
{
    private readonly IAuthAppService _authAppService = Substitute.For<IAuthAppService>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        var opcoes = Options.Create(new JwtOptions { ExpiracaoMinutos = 60 });
        _sut = new AuthController(_authAppService, _tokenGenerator, opcoes);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOkComToken()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = new UsuarioResponse(usuarioId, "admin", "Administrador TicketHub", "Administrador");
        _authAppService.AutenticarAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns(usuario);
        _tokenGenerator.GerarToken(usuarioId.ToString(), usuario.Nome, usuario.Papel).Returns("token-fake");

        var resultado = await _sut.Login(new LoginRequest("admin", "admin123"), CancellationToken.None);

        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var corpo = ok.Value.Should().BeOfType<LoginResponse>().Subject;
        corpo.Token.Should().Be("token-fake");
        corpo.Papel.Should().Be("Administrador");
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        _authAppService.AutenticarAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns((UsuarioResponse?)null);

        var resultado = await _sut.Login(new LoginRequest("admin", "senha-errada"), CancellationToken.None);

        resultado.Result.Should().BeOfType<UnauthorizedObjectResult>();
        _tokenGenerator.DidNotReceiveWithAnyArgs().GerarToken(default!, default!, default!);
    }

    [Fact]
    public async Task Registrar_DeveRetornarCreatedComToken()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = new UsuarioResponse(usuarioId, "novo", "Novo Usuario", "Cliente");
        _authAppService.RegistrarAsync(Arg.Any<RegistrarUsuarioRequest>(), Arg.Any<CancellationToken>()).Returns(usuario);
        _tokenGenerator.GerarToken(usuarioId.ToString(), usuario.Nome, usuario.Papel).Returns("token-fake");

        var resultado = await _sut.Registrar(new RegistrarUsuarioRequest("novo", "senha123", "Novo Usuario"), CancellationToken.None);

        var criado = resultado.Result.Should().BeOfType<ObjectResult>().Subject;
        criado.StatusCode.Should().Be(StatusCodes.Status201Created);
        var corpo = criado.Value.Should().BeOfType<LoginResponse>().Subject;
        corpo.Papel.Should().Be("Cliente");
    }
}
