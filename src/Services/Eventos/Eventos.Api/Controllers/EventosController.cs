using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketHub.Auth;

namespace Eventos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EventosController(IEventoAppService eventoAppService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Papeis.Administrador)]
    public async Task<ActionResult<EventoResponse>> Criar(
        [FromBody] CriarEventoRequest request,
        CancellationToken cancellationToken)
    {
        var evento = await eventoAppService.CriarAsync(request, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = evento.Id }, evento);
    }

    // Anonimo: consultado pelo Ingressos.Api para validacao cruzada de EventoId, sem token de servico.
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
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
    [Authorize(Roles = Papeis.Administrador)]
    public Task<ActionResult<EventoResponse>> Publicar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, eventoAppService.PublicarAsync, cancellationToken);

    [HttpPost("{id:guid}/cancelar")]
    [Authorize(Roles = Papeis.Administrador)]
    public Task<ActionResult<EventoResponse>> Cancelar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, eventoAppService.CancelarAsync, cancellationToken);

    [HttpPost("{id:guid}/encerrar")]
    [Authorize(Roles = Papeis.Administrador)]
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
