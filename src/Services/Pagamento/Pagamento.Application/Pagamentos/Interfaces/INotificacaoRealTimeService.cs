namespace Pagamento.Application.Pagamentos.Interfaces;

/// <summary>
/// Envia notificações em tempo real para o cliente via SignalR.
/// A implementação fica na camada Api (IHubContext), evitando acoplamento de infra na Application.
/// </summary>
public interface INotificacaoRealTimeService
{
    /// <summary>
    /// Notifica o usuário (identificado por nomeUsuario/email) sobre a mudança de status de um pagamento.
    /// </summary>
    Task NotificarStatusPagamentoAsync(
        string nomeUsuario,
        Guid pagamentoId,
        Guid ingressoId,
        decimal valor,
        string status,
        CancellationToken cancellationToken = default);
}
