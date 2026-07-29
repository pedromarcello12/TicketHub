using MassTransit;
using Pagamento.Application.Pagamentos.Interfaces;
using TicketHub.MessageBus.Eventos;

namespace Pagamento.Infrastructure.Integracao;

/// <summary>
/// Publica eventos de pagamento via MassTransit → RabbitMQ.
/// IPublishEndpoint é injetado automaticamente pelo MassTransit (fanout para todos os consumers).
/// </summary>
public class PagamentoEventoPublisher(IPublishEndpoint publishEndpoint) : IPagamentoEventoPublisher
{
    public Task PublicarStatusAlteradoAsync(
        Guid pagamentoId,
        Guid ingressoId,
        decimal valor,
        string status,
        string emailCliente,
        CancellationToken cancellationToken)
    {
        var evento = new PagamentoStatusAlteradoEvent
        {
            PagamentoId = pagamentoId,
            IngressoId = ingressoId,
            Valor = valor,
            Status = status,
            EmailCliente = emailCliente
        };

        return publishEndpoint.Publish(evento, cancellationToken);
    }
}
