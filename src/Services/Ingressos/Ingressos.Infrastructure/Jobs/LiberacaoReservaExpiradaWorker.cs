using Ingressos.Application.Ingressos.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ingressos.Infrastructure.Jobs;

public class LiberacaoReservaExpiradaWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<LiberacaoReservaExpiradaWorker> logger) : BackgroundService
{
    private static readonly TimeSpan IntervaloVarredura = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(IntervaloVarredura);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var quantidade = await sender.Send(new LiberarReservasExpiradasCommand(), stoppingToken);

                if (quantidade > 0)
                    logger.LogInformation("Reservas expiradas liberadas: {Quantidade}", quantidade);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao liberar reservas expiradas de ingressos");
            }
        }
    }
}
