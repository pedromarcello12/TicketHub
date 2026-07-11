using Microsoft.AspNetCore.Mvc;
using Pagamento.Application.Pagamentos.DTOs;
using Pagamento.Application.Pagamentos.Interfaces;

namespace Pagamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagamentosController(IPagamentoAppService pagamentoAppService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PagamentoResponse>> Criar(
        [FromBody] CriarPagamentoRequest request,
        CancellationToken cancellationToken)
    {
        var pagamento = await pagamentoAppService.CriarAsync(request, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = pagamento.Id }, pagamento);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PagamentoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await pagamentoAppService.ObterPorIdAsync(id, cancellationToken);

        return pagamento is null ? NotFound() : Ok(pagamento);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PagamentoResponse>>> Listar(
        [FromQuery] Guid? ingressoId,
        CancellationToken cancellationToken)
    {
        var pagamentos = await pagamentoAppService.ListarAsync(ingressoId, cancellationToken);

        return Ok(pagamentos);
    }

    [HttpPost("{id:guid}/aprovar")]
    public Task<ActionResult<PagamentoResponse>> Aprovar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, pagamentoAppService.AprovarAsync, cancellationToken);

    [HttpPost("{id:guid}/recusar")]
    public Task<ActionResult<PagamentoResponse>> Recusar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, pagamentoAppService.RecusarAsync, cancellationToken);

    [HttpPost("{id:guid}/estornar")]
    public Task<ActionResult<PagamentoResponse>> Estornar(Guid id, CancellationToken cancellationToken) =>
        AplicarTransicaoAsync(id, pagamentoAppService.EstornarAsync, cancellationToken);

    private async Task<ActionResult<PagamentoResponse>> AplicarTransicaoAsync(
        Guid id,
        Func<Guid, CancellationToken, Task<PagamentoResponse?>> transicao,
        CancellationToken cancellationToken)
    {
        try
        {
            var pagamento = await transicao(id, cancellationToken);

            return pagamento is null ? NotFound() : Ok(pagamento);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }
}
