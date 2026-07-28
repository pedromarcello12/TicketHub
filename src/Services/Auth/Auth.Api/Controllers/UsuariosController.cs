using Auth.Application.Auth.Commands;
using Auth.Application.Auth.DTOs;
using Auth.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketHub.Auth;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = Papeis.Administrador)]
public class UsuariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponse>>> Listar(CancellationToken cancellationToken)
    {
        var usuarios = await sender.Send(new ListarUsuariosQuery(), cancellationToken);
        return Ok(usuarios);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var excluido = await sender.Send(new ExcluirUsuarioCommand(id), cancellationToken);
        return excluido ? NoContent() : NotFound();
    }
}
