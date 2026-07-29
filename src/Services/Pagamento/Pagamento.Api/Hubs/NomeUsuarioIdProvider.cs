using Microsoft.AspNetCore.SignalR;

namespace Pagamento.Api.Hubs;

/// <summary>
/// Faz o SignalR usar o claim "nomeUsuario" como identificador do usuário
/// em vez do padrão (ClaimTypes.NameIdentifier / "sub").
///
/// Isso permite enviar notificações via hubContext.Clients.User(nomeUsuario)
/// e agrupar conexões por nomeUsuario no OnConnectedAsync.
/// </summary>
public class NomeUsuarioIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst("nomeUsuario")?.Value;
}
