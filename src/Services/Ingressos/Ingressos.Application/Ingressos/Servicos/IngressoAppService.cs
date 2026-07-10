using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Domain.Entidades;

namespace Ingressos.Application.Ingressos.Servicos;

public class IngressoAppService(IIngressoRepositorio repositorio) : IIngressoAppService
{
    public async Task<IngressoResponse> CriarAsync(CriarIngressoRequest request, CancellationToken cancellationToken)
    {
        var ingresso = new Ingresso(request.EventoId, request.TipoIngresso, request.Preco);

        await repositorio.AdicionarAsync(ingresso, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }

    public async Task<IngressoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(id, cancellationToken);

        return ingresso is null ? null : IngressoResponse.DeEntidade(ingresso);
    }
}
