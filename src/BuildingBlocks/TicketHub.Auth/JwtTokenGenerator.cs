using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TicketHub.Auth;

public class JwtTokenGenerator(IOptions<JwtOptions> opcoes) : IJwtTokenGenerator
{
    public string GerarToken(string usuarioId, string nome, string papel)
    {
        var configuracao = opcoes.Value;
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracao.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, nome),
            new Claim(ClaimTypes.Role, papel)
        };

        var token = new JwtSecurityToken(
            issuer: configuracao.Issuer,
            audience: configuracao.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(configuracao.ExpiracaoMinutos),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
