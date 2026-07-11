using Pagamento.Domain.Enums;

namespace Pagamento.Application.Pagamentos.DTOs;

public record CriarPagamentoRequest(Guid IngressoId, decimal Valor, MetodoPagamento Metodo);
