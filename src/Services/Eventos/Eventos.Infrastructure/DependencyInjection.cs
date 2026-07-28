using Eventos.Application.Behaviors;
using Eventos.Application.Eventos.Interfaces;
using Eventos.Infrastructure.Persistencia;
using Eventos.Infrastructure.Repositorios;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructureEventos(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("EventosDb"),
                sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        // Cache distribuído (Redis em produção, memória em dev sem Redis)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
                options.Configuration = redisConnectionString);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<IEventoRepositorio, EventoRepositorio>();

        // MediatR — registra todos os handlers do assembly Application
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Eventos.Application.Eventos.Queries.ListarEventosQuery>();

            // Pipeline: Logging → Caching → Handler
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        return services;
    }
}
