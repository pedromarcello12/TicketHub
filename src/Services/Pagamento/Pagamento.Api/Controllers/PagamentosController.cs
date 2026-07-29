using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pagamento.Application.Pagamentos.Commands;
using Pagamento.Application.Pagamentos.DTOs;
using Pagamento.Application.Pagamentos.Queries;
using TicketHub.Auth;

namespace Pagamento.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PagamentosController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PagamentoResponse>> Criar(
        [FromBody] CriarPagamentoRequest request,
        CancellationToken cancellationToken)
    {
        var pagamento = await sender.Send(
            new CriarPagamentoCommand(request.IngressoId, request.Valor, request.Metodo, request.EmailCliente),
            cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = pagamento.Id }, pagamento);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PagamentoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await sender.Send(new ObterPagamentoPorIdQuery(id), cancellationToken);
        return pagamento is null ? NotFound() : Ok(pagamento);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PagamentoResponse>>> Listar(
        [FromQuery] Guid? ingressoId,
        CancellationToken cancellationToken)
    {
        var pagamentos = await sender.Send(new ListarPagamentosQuery(ingressoId), cancellationToken);
        return Ok(pagamentos);
    }

    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = Papeis.Administrador)]
    public async Task<ActionResult<PagamentoResponse>> Aprovar(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await sender.Send(new AprovarPagamentoCommand(id), cancellationToken);
        return pagamento is null ? NotFound() : Ok(pagamento);
    }

    [HttpPost("{id:guid}/recusar")]
    [Authorize(Roles = Papeis.Administrador)]
    public async Task<ActionResult<PagamentoResponse>> Recusar(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await sender.Send(new RecusarPagamentoCommand(id), cancellationToken);
        return pagamento is null ? NotFound() : Ok(pagamento);
    }

    [HttpPost("{id:guid}/estornar")]
    [Authorize]
    public async Task<ActionResult<PagamentoResponse>> Estornar(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(Papeis.Administrador);

        if (!isAdmin)
        {
            var nomeUsuarioAtual = User.FindFirstValue("nomeUsuario") ?? string.Empty;
            var pagamentoExistente = await sender.Send(new ObterPagamentoPorIdQuery(id), cancellationToken);

            if (pagamentoExistente is null)
                return NotFound();

            if (!string.Equals(pagamentoExistente.EmailCliente, nomeUsuarioAtual, StringComparison.OrdinalIgnoreCase))
                return Forbid();
        }

        var pagamento = await sender.Send(new EstornarPagamentoCommand(id), cancellationToken);
        return pagamento is null ? NotFound() : Ok(pagamento);
    }
}
