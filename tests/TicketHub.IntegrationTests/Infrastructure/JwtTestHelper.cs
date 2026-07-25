using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TicketHub.IntegrationTests.Infrastructure;

public static class JwtTestHelper
{
    private const string SecretKey = "chave-de-teste-suficientemente-longa-para-256-bits";
    private const string Issuer = "TicketHub";
    private const string Audience = "TicketHub";

    public static string GerarTokenAdmin() =>
        GerarToken("admin-id", "Admin Teste", "Administrador");

    public static string GerarTokenUsuario() =>
        GerarToken("user-id", "Usuário Teste", "Usuario");

    private static string GerarToken(string id, string nome, string papel)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, id),
            new Claim(JwtRegisteredClaimNames.Name, nome),
            new Claim(ClaimTypes.Role, papel)
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
