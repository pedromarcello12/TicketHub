using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TicketHub.MessageBus;

public static class DependencyInjection
{
    /// <summary>
    /// Registra MassTransit com transporte RabbitMQ.
    /// Use <paramref name="configurarBus"/> para registrar consumers específicos do serviço.
    /// </summary>
    /// <example>
    /// // Somente publicador (sem consumers):
    /// services.AdicionarMassTransitRabbitMq(configuration);
    ///
    /// // Com consumer:
    /// services.AdicionarMassTransitRabbitMq(configuration, bus =>
    ///     bus.AddConsumer&lt;MeuConsumer, MeuConsumerDefinition&gt;());
    /// </example>
    public static IServiceCollection AdicionarMassTransitRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configurarBus = null)
    {
        var opts = new RabbitMqOptions();
        configuration.GetSection(RabbitMqOptions.SectionName).Bind(opts);

        services.AddMassTransit(bus =>
        {
            // Registra consumers do serviço (ex: AddConsumer<T, TDefinition>)
            configurarBus?.Invoke(bus);

            bus.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(opts.HostName, (ushort)opts.Port, "/", h =>
                {
                    h.Username(opts.UserName);
                    h.Password(opts.Password);
                });

                // Retry global com backoff exponencial para todo o bus
                cfg.UseMessageRetry(r =>
                    r.Exponential(5,
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(60),
                        TimeSpan.FromSeconds(5)));

                // Configura automaticamente endpoints para todos os consumers registrados
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
