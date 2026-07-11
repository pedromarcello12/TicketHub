using Microsoft.Extensions.Options;
using TicketHub.MessageBus;
using TicketHub.MessageBus.Eventos;

namespace Notificacoes.Worker.Workers;

public class PagamentoStatusAlteradoConsumer(
    IOptions<RabbitMqOptions> opcoes,
    ILogger<PagamentoStatusAlteradoConsumer> logger)
    : RabbitMqConsumerBackgroundService<PagamentoStatusAlteradoEvent>(
        opcoes,
        logger,
        RabbitMqConstantes.Filas.NotificacoesPagamentoStatusAlterado,
        RabbitMqConstantes.RoutingKeys.PagamentoStatusAlterado)
{
    protected override Task TratarAsync(PagamentoStatusAlteradoEvent evento, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Notificando usuario: pagamento {PagamentoId} do ingresso {IngressoId} teve status alterado para {Status} (valor {Valor:C})",
            evento.PagamentoId,
            evento.IngressoId,
            evento.Status,
            evento.Valor);

        return Task.CompletedTask;
    }
}
