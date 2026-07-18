using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TicketHub.Auth;

namespace Auth.Api.Controllers;

public record LoginResponse(string Token, string RefreshToken, string Nome, string Papel, DateTime ExpiraEm);

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
        var resultado = await authAppService.AutenticarAsync(request, cancellationToken);
        if (resultado is null)
            return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });

        return Ok(GerarResposta(resultado));
    }

    [HttpPost("registrar")]
    public async Task<ActionResult<LoginResponse>> Registrar([FromBody] RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var resultado = await authAppService.RegistrarAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, GerarResposta(resultado));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var resultado = await authAppService.RenovarTokenAsync(request.RefreshToken, cancellationToken);
        if (resultado is null)
            return Unauthorized(new { mensagem = "Refresh token inválido ou expirado." });

        return Ok(GerarResposta(resultado));
    }

    private LoginResponse GerarResposta(ResultadoAutenticacao resultado)
    {
        var token = tokenGenerator.GerarToken(resultado.Usuario.Id.ToString(), resultado.Usuario.Nome, resultado.Usuario.Papel);
        var expiraEm = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiracaoMinutos);

        return new LoginResponse(token, resultado.RefreshToken, resultado.Usuario.Nome, resultado.Usuario.Papel, expiraEm);
    }
}
