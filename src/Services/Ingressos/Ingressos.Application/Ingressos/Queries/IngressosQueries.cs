using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Interfaces;
using MediatR;

namespace Ingressos.Application.Ingressos.Queries;

// ─── Listar ──────────────────────────────────────────────────────────────────

public record ListarIngressosQuery(Guid? EventoId) : IRequest<IReadOnlyList<IngressoResponse>>;

public class ListarIngressosQueryHandler(IIngressoRepositorio repositorio)
    : IRequestHandler<ListarIngressosQuery, IReadOnlyList<IngressoResponse>>
{
    public async Task<IReadOnlyList<IngressoResponse>> Handle(ListarIngressosQuery request, CancellationToken cancellationToken)
    {
        var ingressos = await repositorio.ListarAsync(request.EventoId, cancellationToken);
        return ingressos.Select(IngressoResponse.DeEntidade).ToList();
    }
}

// ─── ObterPorId ───────────────────────────────────────────────────────────────

public record ObterIngressoPorIdQuery(Guid Id) : IRequest<IngressoResponse?>;

public class ObterIngressoPorIdQueryHandler(IIngressoRepositorio repositorio)
    : IRequestHandler<ObterIngressoPorIdQuery, IngressoResponse?>
{
    public async Task<IngressoResponse?> Handle(ObterIngressoPorIdQuery request, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return ingresso is null ? null : IngressoResponse.DeEntidade(ingresso);
    }
}
