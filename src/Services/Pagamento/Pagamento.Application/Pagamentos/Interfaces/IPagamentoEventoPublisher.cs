namespace Pagamento.Application.Pagamentos.Interfaces;

public interface IPagamentoEventoPublisher
{
    Task PublicarStatusAlteradoAsync(
        Guid pagamentoId,
        Guid ingressoId,
        decimal valor,
        string status,
        CancellationToken cancellationToken);
}
