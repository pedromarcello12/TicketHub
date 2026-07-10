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
}
