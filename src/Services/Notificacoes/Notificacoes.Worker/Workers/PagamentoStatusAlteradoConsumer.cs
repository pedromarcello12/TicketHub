using Microsoft.Extensions.Options;
using Notificacoes.Worker.Email;
using TicketHub.MessageBus;
using TicketHub.MessageBus.Eventos;

namespace Notificacoes.Worker.Workers;

public class PagamentoStatusAlteradoConsumer(
    IOptions<RabbitMqOptions> opcoes,
    ILogger<PagamentoStatusAlteradoConsumer> logger,
    IEmailSender emailSender)
    : RabbitMqConsumerBackgroundService<PagamentoStatusAlteradoEvent>(
        opcoes,
        logger,
        RabbitMqConstantes.Filas.NotificacoesPagamentoStatusAlterado,
        RabbitMqConstantes.RoutingKeys.PagamentoStatusAlterado)
{
    protected override async Task TratarAsync(PagamentoStatusAlteradoEvent evento, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Notificando usuario: pagamento {PagamentoId} do ingresso {IngressoId} teve status alterado para {Status} (valor {Valor:C})",
            evento.PagamentoId,
            evento.IngressoId,
            evento.Status,
            evento.Valor);

        var assunto = $"TicketHub - Pagamento {evento.Status}";
        var corpo = $"""
            Olá!

            O status do seu pagamento (id {evento.PagamentoId}) referente ao ingresso {evento.IngressoId} foi atualizado para: {evento.Status}.

            Valor: {evento.Valor:C}

            Obrigado por usar o TicketHub.
            """;

        await emailSender.EnviarAsync(evento.EmailCliente, assunto, corpo, cancellationToken);
    }
}
