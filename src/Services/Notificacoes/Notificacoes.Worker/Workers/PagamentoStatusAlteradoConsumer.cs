using Microsoft.Extensions.Options;
using Notificacoes.Worker.Email;
using Notificacoes.Worker.QrCode;
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
    private const string StatusAprovado = "Aprovado";

    protected override async Task TratarAsync(PagamentoStatusAlteradoEvent evento, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Notificando usuario: pagamento {PagamentoId} do ingresso {IngressoId} teve status alterado para {Status} (valor {Valor:C})",
            evento.PagamentoId,
            evento.IngressoId,
            evento.Status,
            evento.Valor);

        var assunto = $"TicketHub — Pagamento {evento.Status}";
        var corpo = evento.Status == StatusAprovado
            ? MontarEmailAprovado(evento)
            : MontarEmailStatus(evento);

        await emailSender.EnviarAsync(evento.EmailCliente, assunto, corpo, cancellationToken, isHtml: true);
    }

    private static string MontarEmailAprovado(PagamentoStatusAlteradoEvent evento)
    {
        var qrConteudo = $"tickethub://ingressos/{evento.IngressoId}";
        var qrBase64 = QrCodeService.GerarBase64(qrConteudo);

        return $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="UTF-8"><style>
              body {{ font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
              .card {{ background: white; border-radius: 8px; padding: 32px; max-width: 480px; margin: 0 auto; box-shadow: 0 2px 8px rgba(0,0,0,.1); }}
              h2 {{ color: #1a1a1a; margin-top: 0; }}
              .badge {{ display: inline-block; background: #16a34a; color: white; padding: 4px 12px; border-radius: 20px; font-size: 14px; font-weight: bold; }}
              .info {{ color: #555; font-size: 14px; margin: 16px 0; }}
              .qr-section {{ text-align: center; margin: 24px 0; }}
              .qr-section img {{ border: 1px solid #ddd; border-radius: 8px; padding: 8px; }}
              .qr-label {{ color: #888; font-size: 12px; margin-top: 8px; }}
              .footer {{ color: #aaa; font-size: 12px; margin-top: 24px; border-top: 1px solid #eee; padding-top: 16px; }}
            </style></head>
            <body>
              <div class="card">
                <h2>🎉 Ingresso Confirmado!</h2>
                <p class="badge">Pagamento Aprovado</p>
                <div class="info">
                  <p><strong>Ingresso:</strong> {evento.IngressoId}</p>
                  <p><strong>Pagamento:</strong> {evento.PagamentoId}</p>
                  <p><strong>Valor:</strong> {evento.Valor:C}</p>
                </div>
                <div class="qr-section">
                  <p><strong>Seu QR Code de acesso:</strong></p>
                  <img src="data:image/png;base64,{qrBase64}" width="200" height="200" alt="QR Code do ingresso" />
                  <p class="qr-label">Apresente este código na entrada do evento</p>
                </div>
                <div class="footer">TicketHub — Obrigado pela sua compra!</div>
              </div>
            </body>
            </html>
            """;
    }

    private static string MontarEmailStatus(PagamentoStatusAlteradoEvent evento)
    {
        var cor = evento.Status == "Recusado" ? "#dc2626" : "#d97706";
        return $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="UTF-8"><style>
              body {{ font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
              .card {{ background: white; border-radius: 8px; padding: 32px; max-width: 480px; margin: 0 auto; box-shadow: 0 2px 8px rgba(0,0,0,.1); }}
              h2 {{ color: #1a1a1a; margin-top: 0; }}
              .badge {{ display: inline-block; background: {cor}; color: white; padding: 4px 12px; border-radius: 20px; font-size: 14px; font-weight: bold; }}
              .info {{ color: #555; font-size: 14px; margin: 16px 0; }}
              .footer {{ color: #aaa; font-size: 12px; margin-top: 24px; border-top: 1px solid #eee; padding-top: 16px; }}
            </style></head>
            <body>
              <div class="card">
                <h2>Atualização do seu Pagamento</h2>
                <p class="badge">{evento.Status}</p>
                <div class="info">
                  <p><strong>Ingresso:</strong> {evento.IngressoId}</p>
                  <p><strong>Pagamento:</strong> {evento.PagamentoId}</p>
                  <p><strong>Valor:</strong> {evento.Valor:C}</p>
                </div>
                <div class="footer">TicketHub — Em caso de dúvidas, entre em contato com nosso suporte.</div>
              </div>
            </body>
            </html>
            """;
    }
}
