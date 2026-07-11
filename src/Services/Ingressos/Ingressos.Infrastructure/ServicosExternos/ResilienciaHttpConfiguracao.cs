using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Ingressos.Infrastructure.ServicosExternos;

public static class ResilienciaHttpConfiguracao
{
    public static void Configurar(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(4);
        options.Retry.MaxRetryAttempts = 2;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
    }
}
