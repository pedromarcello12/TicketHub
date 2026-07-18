using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TicketHub.Auth;

namespace Auth.Api.Controllers;

public record LoginResponse(string Token, string Nome, string Papel, DateTime ExpiraEm);

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimitingPolicies.Login)]
public class AuthController(
    IAuthAppService authAppService,
    IJwtTokenGenerator tokenGenerator,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await authAppService.AutenticarAsync(request, cancellationToken);
        if (usuario is null)
            return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });

        return Ok(GerarResposta(usuario));
    }

    [HttpPost("registrar")]
    public async Task<ActionResult<LoginResponse>> Registrar([FromBody] RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await authAppService.RegistrarAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, GerarResposta(usuario));
    }

    private LoginResponse GerarResposta(UsuarioResponse usuario)
    {
        var token = tokenGenerator.GerarToken(usuario.Id.ToString(), usuario.Nome, usuario.Papel);
        var expiraEm = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiracaoMinutos);

        return new LoginResponse(token, usuario.Nome, usuario.Papel, expiraEm);
    }
}
