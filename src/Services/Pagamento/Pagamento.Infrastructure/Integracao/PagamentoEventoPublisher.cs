using Pagamento.Application.Pagamentos.Interfaces;
using TicketHub.MessageBus;
using TicketHub.MessageBus.Eventos;

namespace Pagamento.Infrastructure.Integracao;

public class PagamentoEventoPublisher(IEventoPublisher eventoPublisher) : IPagamentoEventoPublisher
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

        eventoPublisher.Publicar(evento, RabbitMqConstantes.RoutingKeys.PagamentoStatusAlterado);

        return Task.CompletedTask;
    }
}
