using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace TicketHub.MessageBus;

public static class RabbitMqRetryPolicies
{
    public static ISyncPolicy CriarPoliticaConexao(ILogger logger) =>
        Policy
            .Handle<Exception>()
            .WaitAndRetry(
                retryCount: 5,
                sleepDurationProvider: tentativa => TimeSpan.FromSeconds(Math.Pow(2, tentativa)),
                onRetry: (ex, espera, tentativa, _) =>
                    logger.LogWarning(
                        ex,
                        "Falha ao conectar ao RabbitMQ (tentativa {Tentativa}/5). Nova tentativa em {Espera}.",
                        tentativa,
                        espera));

    public static ISyncPolicy CriarPoliticaPublicacao(ILogger logger) =>
        Policy
            .Handle<Exception>()
            .WaitAndRetry(
                retryCount: 3,
                sleepDurationProvider: _ => TimeSpan.FromMilliseconds(200),
                onRetry: (ex, espera, tentativa, _) =>
                    logger.LogWarning(
                        ex,
                        "Falha ao publicar evento no RabbitMQ (tentativa {Tentativa}/3). Nova tentativa em {Espera}.",
                        tentativa,
                        espera));
}
