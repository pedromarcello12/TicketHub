using Pagamento.Application.Pagamentos.DTOs;

namespace Pagamento.Application.Pagamentos.Interfaces;

public interface IPagamentoAppService
{
    Task<PagamentoResponse> CriarAsync(CriarPagamentoRequest request, CancellationToken cancellationToken);
    Task<PagamentoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PagamentoResponse>> ListarAsync(Guid? ingressoId, CancellationToken cancellationToken);
    Task<PagamentoResponse?> AprovarAsync(Guid id, CancellationToken cancellationToken);
    Task<PagamentoResponse?> RecusarAsync(Guid id, CancellationToken cancellationToken);
    Task<PagamentoResponse?> EstornarAsync(Guid id, CancellationToken cancellationToken);
}
