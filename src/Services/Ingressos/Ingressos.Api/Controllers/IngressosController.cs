using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ingressos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngressosController(IIngressoAppService ingressoAppService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<IngressoResponse>> Criar(
        [FromBody] CriarIngressoRequest request,
        CancellationToken cancellationToken)
    {
        var ingresso = await ingressoAppService.CriarAsync(request, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = ingresso.Id }, ingresso);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IngressoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await ingressoAppService.ObterPorIdAsync(id, cancellationToken);

        return ingresso is null ? NotFound() : Ok(ingresso);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngressoResponse>>> Listar(
        [FromQuery] Guid? eventoId,
        CancellationToken cancellationToken)
    {
        var ingressos = await ingressoAppService.ListarAsync(eventoId, cancellationToken);

        return Ok(ingressos);
    }

    [HttpPost("{id:guid}/reservar")]
    public Task<ActionResult<IngressoResponse>> Reservar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, ingressoAppService.ReservarAsync, cancellationToken);

    [HttpPost("{id:guid}/confirmar-venda")]
    public Task<ActionResult<IngressoResponse>> ConfirmarVenda(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, ingressoAppService.ConfirmarVendaAsync, cancellationToken);

    [HttpPost("{id:guid}/cancelar")]
    public Task<ActionResult<IngressoResponse>> Cancelar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, ingressoAppService.CancelarAsync, cancellationToken);

    private async Task<ActionResult<IngressoResponse>> AplicarTransicaoAsync(
        Guid id,
        Func<Guid, CancellationToken, Task<IngressoResponse?>> transicao,
        CancellationToken cancellationToken)
    {
        try
        {
            var ingresso = await transicao(id, cancellationToken);

            return ingresso is null ? NotFound() : Ok(ingresso);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }
}
