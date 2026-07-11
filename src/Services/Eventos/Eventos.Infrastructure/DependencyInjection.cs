using Eventos.Application.Eventos.Interfaces;
using Eventos.Application.Eventos.Servicos;
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

        services.AddScoped<IEventoRepositorio, EventoRepositorio>();
        services.AddScoped<IEventoAppService, EventoAppService>();

        return services;
    }
}
