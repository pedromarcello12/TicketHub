using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TicketHub.Auth;
using Xunit;

namespace Auth.Tests;

public class JwtTokenGeneratorTests
{
    private static JwtTokenGenerator CriarGerador(JwtOptions? opcoes = null) =>
        new(Options.Create(opcoes ?? new JwtOptions
        {
            SecretKey = "chave-de-teste-com-pelo-menos-32-caracteres-para-hmac-sha256",
            Issuer = "TicketHub.Testes",
            Audience = "TicketHub.Testes",
            ExpiracaoMinutos = 30
        }));

    [Fact]
    public void GerarToken_DeveIncluirClaimsCorretas()
    {
        var gerador = CriarGerador();

        var token = gerador.GerarToken("42", "Fulano de Tal", "Administrador");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Subject.Should().Be("42");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "Fulano de Tal");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Administrador");
    }

    [Fact]
    public void GerarToken_DeveUsarIssuerEAudienceConfigurados()
    {
        var opcoes = new JwtOptions
        {
            SecretKey = "chave-de-teste-com-pelo-menos-32-caracteres-para-hmac-sha256",
            Issuer = "MeuIssuer",
            Audience = "MinhaAudience",
            ExpiracaoMinutos = 30
        };
        var gerador = CriarGerador(opcoes);

        var token = gerador.GerarToken("1", "Nome", "Cliente");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("MeuIssuer");
        jwt.Audiences.Should().Contain("MinhaAudience");
    }

    [Fact]
    public void GerarToken_DeveExpirarConformeConfiguracao()
    {
        var opcoes = new JwtOptions
        {
            SecretKey = "chave-de-teste-com-pelo-menos-32-caracteres-para-hmac-sha256",
            ExpiracaoMinutos = 15
        };
        var gerador = CriarGerador(opcoes);

        var token = gerador.GerarToken("1", "Nome", "Cliente");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GerarToken_DeveGerarJtiUnicoACadaChamada()
    {
        var gerador = CriarGerador();

        var token1 = gerador.GerarToken("1", "Nome", "Cliente");
        var token2 = gerador.GerarToken("1", "Nome", "Cliente");

        var jti1 = new JwtSecurityTokenHandler().ReadJwtToken(token1).Id;
        var jti2 = new JwtSecurityTokenHandler().ReadJwtToken(token2).Id;
        jti1.Should().NotBe(jti2);
    }
}
