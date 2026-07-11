using Auth.Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using TicketHub.Auth;
using Xunit;

namespace Auth.Tests;

public class AuthControllerTests
{
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        var opcoes = Options.Create(new JwtOptions { ExpiracaoMinutos = 60 });
        _sut = new AuthController(_tokenGenerator, opcoes);
    }

    [Fact]
    public void Login_ComCredenciaisValidas_DeveRetornarOkComToken()
    {
        _tokenGenerator.GerarToken("1", "Administrador TicketHub", "Administrador").Returns("token-fake");

        var resultado = _sut.Login(new LoginRequest("admin", "admin123"));

        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var corpo = ok.Value.Should().BeOfType<LoginResponse>().Subject;
        corpo.Token.Should().Be("token-fake");
        corpo.Papel.Should().Be("Administrador");
    }

    [Fact]
    public void Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        var resultado = _sut.Login(new LoginRequest("admin", "senha-errada"));

        resultado.Result.Should().BeOfType<UnauthorizedObjectResult>();
        _tokenGenerator.DidNotReceiveWithAnyArgs().GerarToken(default!, default!, default!);
    }
}
