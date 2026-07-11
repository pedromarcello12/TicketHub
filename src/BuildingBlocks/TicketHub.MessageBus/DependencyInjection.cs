using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TicketHub.MessageBus;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IEventoPublisher, RabbitMqEventoPublisher>();

        return services;
    }
}
