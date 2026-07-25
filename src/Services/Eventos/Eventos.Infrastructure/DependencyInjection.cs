using Eventos.Application.Eventos.Interfaces;
using Eventos.Application.Eventos.Servicos;
using Eventos.Infrastructure.Cache;
using Eventos.Infrastructure.Persistencia;
using Eventos.Infrastructure.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructureEventos(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("EventosDb")));

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
        services.AddScoped<EventoAppService>();
        services.AddScoped<IEventoAppService>(sp =>
            new CachedEventoAppService(
                sp.GetRequiredService<EventoAppService>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>()));

        return services;
    }
}
