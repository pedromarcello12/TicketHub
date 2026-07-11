using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Eventos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventosController(IEventoAppService eventoAppService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EventoResponse>> Criar(
        [FromBody] CriarEventoRequest request,
        CancellationToken cancellationToken)
    {
        var evento = await eventoAppService.CriarAsync(request, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = evento.Id }, evento);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var evento = await eventoAppService.ObterPorIdAsync(id, cancellationToken);

        return evento is null ? NotFound() : Ok(evento);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventoResponse>>> Listar(CancellationToken cancellationToken)
    {
        var eventos = await eventoAppService.ListarAsync(cancellationToken);

        return Ok(eventos);
    }

    [HttpPost("{id:guid}/publicar")]
    public Task<ActionResult<EventoResponse>> Publicar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, eventoAppService.PublicarAsync, cancellationToken);

    [HttpPost("{id:guid}/cancelar")]
    public Task<ActionResult<EventoResponse>> Cancelar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, eventoAppService.CancelarAsync, cancellationToken);

    [HttpPost("{id:guid}/encerrar")]
    public Task<ActionResult<EventoResponse>> Encerrar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, eventoAppService.EncerrarAsync, cancellationToken);

    private async Task<ActionResult<EventoResponse>> AplicarTransicaoAsync(
        Guid id,
        Func<Guid, CancellationToken, Task<EventoResponse?>> transicao,
        CancellationToken cancellationToken)
    {
        try
        {
            var evento = await transicao(id, cancellationToken);

            return evento is null ? NotFound() : Ok(evento);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }
}
