using Ingressos.Application.Ingressos.Commands;
using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketHub.Auth;

namespace Ingressos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IngressosController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Papeis.Administrador)]
    public async Task<ActionResult<IngressoResponse>> Criar(
        [FromBody] CriarIngressoRequest request,
        CancellationToken cancellationToken)
    {
        var ingresso = await sender.Send(
            new CriarIngressoCommand(request.EventoId, request.TipoIngresso, request.Preco),
            cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = ingresso.Id }, ingresso);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IngressoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await sender.Send(new ObterIngressoPorIdQuery(id), cancellationToken);
        return ingresso is null ? NotFound() : Ok(ingresso);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngressoResponse>>> Listar(
        [FromQuery] Guid? eventoId,
        CancellationToken cancellationToken)
    {
        var ingressos = await sender.Send(new ListarIngressosQuery(eventoId), cancellationToken);
        return Ok(ingressos);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Papeis.Administrador)]
    public async Task<ActionResult<IngressoResponse>> Atualizar(
        Guid id,
        [FromBody] AtualizarIngressoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ingresso = await sender.Send(
                new AtualizarIngressoCommand(id, request.TipoIngresso, request.Preco),
                cancellationToken);

            return ingresso is null ? NotFound() : Ok(ingresso);
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
            var excluido = await sender.Send(new ExcluirIngressoCommand(id), cancellationToken);
            return excluido ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reservar")]
    public async Task<ActionResult<IngressoResponse>> Reservar(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await sender.Send(new ReservarIngressoCommand(id), cancellationToken);
        return ingresso is null ? NotFound() : Ok(ingresso);
    }

    [HttpPost("{id:guid}/confirmar-venda")]
    public async Task<ActionResult<IngressoResponse>> ConfirmarVenda(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await sender.Send(new ConfirmarVendaIngressoCommand(id), cancellationToken);
        return ingresso is null ? NotFound() : Ok(ingresso);
    }

    [HttpPost("{id:guid}/cancelar")]
    public async Task<ActionResult<IngressoResponse>> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await sender.Send(new CancelarIngressoCommand(id), cancellationToken);
        return ingresso is null ? NotFound() : Ok(ingresso);
    }
}
