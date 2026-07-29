using Eventos.Application.Eventos.Commands;
using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketHub.Auth;

namespace Eventos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EventosController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Papeis.Administrador)]
    public async Task<ActionResult<EventoResponse>> Criar(
        [FromBody] CriarEventoRequest request,
        CancellationToken cancellationToken)
    {
        var evento = await sender.Send(
            new CriarEventoCommand(request.Nome, request.Local, request.DataHora, request.CapacidadeTotal),
            cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = evento.Id }, evento);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var evento = await sender.Send(new ObterEventoPorIdQuery(id), cancellationToken);
        return evento is null ? NotFound() : Ok(evento);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventoResponse>>> Listar(CancellationToken cancellationToken)
    {
        var eventos = await sender.Send(new ListarEventosQuery(), cancellationToken);
        return Ok(eventos);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Papeis.Administrador)]
    public async Task<ActionResult<EventoResponse>> Atualizar(
        Guid id,
        [FromBody] CriarEventoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var evento = await sender.Send(
                new AtualizarEventoCommand(id, request.Nome, request.Local, request.DataHora, request.CapacidadeTotal),
                cancellationToken);

            return evento is null ? NotFound() : Ok(evento);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Papeis.Administrador)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var excluido = await sender.Send(new ExcluirEventoCommand(id), cancellationToken);
            return excluido ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [HttpPost("{id:guid}/publicar")]
    [Authorize(Roles = Papeis.Administrador)]
    public Task<ActionResult<EventoResponse>> Publicar(Guid id, CancellationToken cancellationToken) =>
        EnviarTransicaoAsync(new PublicarEventoCommand(id), cancellationToken);

    [HttpPost("{id:guid}/cancelar")]
    [Authorize(Roles = Papeis.Administrador)]
    public Task<ActionResult<EventoResponse>> Cancelar(Guid id, CancellationToken cancellationToken) =>
        EnviarTransicaoAsync(new CancelarEventoCommand(id), cancellationToken);

    [HttpPost("{id:guid}/encerrar")]
    [Authorize(Roles = Papeis.Administrador)]
    public Task<ActionResult<EventoResponse>> Encerrar(Guid id, CancellationToken cancellationToken) =>
        EnviarTransicaoAsync(new EncerrarEventoCommand(id), cancellationToken);

    private async Task<ActionResult<EventoResponse>> EnviarTransicaoAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : IRequest<EventoResponse?>
    {
        try
        {
            var evento = await sender.Send(command, cancellationToken);
            return evento is null ? NotFound() : Ok(evento);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }
}
