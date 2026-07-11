namespace Pagamento.Application.Pagamentos.Interfaces;

public interface IIngressoExternalService
{
    Task<bool> ExisteAsync(Guid ingressoId, CancellationToken cancellationToken);
}
