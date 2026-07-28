using MediatR;
using Pagamento.Application.Pagamentos.DTOs;
using Pagamento.Application.Pagamentos.Interfaces;

namespace Pagamento.Application.Pagamentos.Queries;

// ─── Listar ──────────────────────────────────────────────────────────────────

public record ListarPagamentosQuery(Guid? IngressoId) : IRequest<IReadOnlyList<PagamentoResponse>>;

public class ListarPagamentosQueryHandler(IPagamentoRepositorio repositorio)
    : IRequestHandler<ListarPagamentosQuery, IReadOnlyList<PagamentoResponse>>
{
    public async Task<IReadOnlyList<PagamentoResponse>> Handle(ListarPagamentosQuery request, CancellationToken cancellationToken)
    {
        var pagamentos = await repositorio.ListarAsync(request.IngressoId, cancellationToken);
        return pagamentos.Select(PagamentoResponse.DeEntidade).ToList();
    }
}

// ─── ObterPorId ───────────────────────────────────────────────────────────────

public record ObterPagamentoPorIdQuery(Guid Id) : IRequest<PagamentoResponse?>;

public class ObterPagamentoPorIdQueryHandler(IPagamentoRepositorio repositorio)
    : IRequestHandler<ObterPagamentoPorIdQuery, PagamentoResponse?>
{
    public async Task<PagamentoResponse?> Handle(ObterPagamentoPorIdQuery request, CancellationToken cancellationToken)
    {
        var pagamento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return pagamento is null ? null : PagamentoResponse.DeEntidade(pagamento);
    }
}
