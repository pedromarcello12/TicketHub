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
    public async Task<ActionResult<IngressoResponse>> Reservar(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await ingressoAppService.ReservarAsync(id, cancellationToken);

        return ingresso is null ? NotFound() : Ok(ingresso);
    }

    [HttpPost("{id:guid}/confirmar-venda")]
    public async Task<ActionResult<IngressoResponse>> ConfirmarVenda(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await ingressoAppService.ConfirmarVendaAsync(id, cancellationToken);

        return ingresso is null ? NotFound() : Ok(ingresso);
    }

    [HttpPost("{id:guid}/cancelar")]
    public async Task<ActionResult<IngressoResponse>> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await ingressoAppService.CancelarAsync(id, cancellationToken);

        return ingresso is null ? NotFound() : Ok(ingresso);
    }
}
