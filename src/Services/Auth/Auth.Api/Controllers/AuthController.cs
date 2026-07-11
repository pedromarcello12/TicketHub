using Auth.Api.Usuarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TicketHub.Auth;

namespace Auth.Api.Controllers;

public record LoginRequest(string NomeUsuario, string Senha);

public record LoginResponse(string Token, string Nome, string Papel, DateTime ExpiraEm);

[ApiController]
[Route("api/auth")]
public class AuthController(IJwtTokenGenerator tokenGenerator, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var usuario = UsuariosSeed.Autenticar(request.NomeUsuario, request.Senha);
        if (usuario is null)
            return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });

        var token = tokenGenerator.GerarToken(usuario.Id, usuario.Nome, usuario.Papel);
        var expiraEm = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiracaoMinutos);

        return Ok(new LoginResponse(token, usuario.Nome, usuario.Papel, expiraEm));
    }
}
