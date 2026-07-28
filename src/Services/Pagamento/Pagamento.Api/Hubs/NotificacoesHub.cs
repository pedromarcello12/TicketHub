using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pagamento.Application.Pagamentos.Interfaces;

namespace Pagamento.Api.Hubs;

/// <summary>
/// Hub SignalR para notificações em tempo real de pagamentos.
/// Cada usuário autenticado recebe notificações apenas para os seus próprios pagamentos.
///
/// Grupos: cada cliente entra no grupo com seu nomeUsuario (claim "nomeUsuario" do JWT).
/// Assim, ao aprovar/recusar/estornar, enviamos apenas para o grupo do dono do pagamento.
/// </summary>
[Authorize]
public class NotificacoesHub : Hub
{
    /// <summary>
    /// Ao conectar, o usuário entra automaticamente no grupo com seu nomeUsuario.
    /// O frontend não precisa fazer nada — a associação é automática e segura.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var nomeUsuario = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(nomeUsuario))
            await Groups.AddToGroupAsync(Context.ConnectionId, nomeUsuario);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var nomeUsuario = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(nomeUsuario))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nomeUsuario);

        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Implementa INotificacaoRealTimeService usando IHubContext do SignalR.
/// Registrado no Pagamento.Api (não na Infrastructure) para evitar dependência circular.
/// </summary>
public class SignalRNotificacaoService(IHubContext<NotificacoesHub> hubContext)
    : INotificacaoRealTimeService
{
    public Task NotificarStatusPagamentoAsync(
        string nomeUsuario,
        Guid pagamentoId,
        Guid ingressoId,
        decimal valor,
        string status,
        CancellationToken cancellationToken = default)
    {
        // Envia para todos os clients do grupo (nomeUsuario) — mesmo em múltiplas abas/dispositivos
        return hubContext.Clients
            .Group(nomeUsuario)
            .SendAsync("PagamentoStatusAlterado", new
            {
                pagamentoId,
                ingressoId,
                valor,
                status
            }, cancellationToken);
    }
}
