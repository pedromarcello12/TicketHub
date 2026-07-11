using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Application.Pagamentos.Interfaces;

public interface IPagamentoRepositorio
{
    Task AdicionarAsync(EntidadePagamento pagamento, CancellationToken cancellationToken);
    Task<EntidadePagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<EntidadePagamento>> ListarAsync(Guid? ingressoId, CancellationToken cancellationToken);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
