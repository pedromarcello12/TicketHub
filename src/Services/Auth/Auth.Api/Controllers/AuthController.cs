using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Application.Auth.Commands;
using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TicketHub.Auth;

namespace Auth.Api.Controllers;

public record LoginResponse(string Token, string RefreshToken, string Nome, string Papel, string Email, DateTime ExpiraEm);

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimitingPolicies.Login)]
public class AuthController(
    ISender sender,
    IJwtTokenGenerator tokenGenerator,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new LoginCommand(request.NomeUsuario, request.Senha), cancellationToken);
        if (resultado is null)
            return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });

        return Ok(GerarResposta(resultado));
    }

    [HttpPost("registrar")]
    public async Task<ActionResult<LoginResponse>> Registrar([FromBody] RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(
            new RegistrarUsuarioCommand(request.NomeUsuario, request.Senha, request.Nome, request.Papel),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, GerarResposta(resultado));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new RenovarTokenCommand(request.RefreshToken), cancellationToken);
        if (resultado is null)
            return Unauthorized(new { mensagem = "Refresh token inválido ou expirado." });

        return Ok(GerarResposta(resultado));
    }

    [HttpGet("me")]
    [Authorize]
    [DisableRateLimiting]
    public async Task<ActionResult<UsuarioResponse>> ObterPerfil(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var usuarioId))
            return Unauthorized();

        var perfil = await sender.Send(new ObterPerfilQuery(usuarioId), cancellationToken);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpPatch("me")]
    [Authorize]
    [DisableRateLimiting]
    public async Task<ActionResult<UsuarioResponse>> AtualizarPerfil(
        [FromBody] AtualizarPerfilRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var usuarioId))
            return Unauthorized();

        try
        {
            var perfil = await sender.Send(
                new AtualizarPerfilCommand(usuarioId, request.Nome, request.SenhaAtual, request.NovaSenha),
                cancellationToken);

            return perfil is null ? NotFound() : Ok(perfil);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    private LoginResponse GerarResposta(ResultadoAutenticacao resultado)
    {
        var token = tokenGenerator.GerarToken(
            resultado.Usuario.Id.ToString(),
            resultado.Usuario.NomeUsuario,
            resultado.Usuario.Nome,
            resultado.Usuario.Papel);

        var expiraEm = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiracaoMinutos);

        return new LoginResponse(token, resultado.RefreshToken, resultado.Usuario.Nome, resultado.Usuario.Papel, resultado.Usuario.Email, expiraEm);
    }
}
