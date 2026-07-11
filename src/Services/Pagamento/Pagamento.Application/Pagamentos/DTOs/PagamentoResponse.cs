using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Application.Pagamentos.DTOs;

public record PagamentoResponse(
    Guid Id,
    Guid IngressoId,
    decimal Valor,
    string Metodo,
    string Status)
{
    public static PagamentoResponse DeEntidade(EntidadePagamento pagamento) => new(
        pagamento.Id,
        pagamento.IngressoId,
        pagamento.Valor,
        pagamento.Metodo.ToString(),
        pagamento.Status.ToString());
}
