using Ingressos.Application.Behaviors;
using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Infrastructure.Jobs;
using Ingressos.Infrastructure.Persistencia;
using Ingressos.Infrastructure.Repositorios;
using Ingressos.Infrastructure.ServicosExternos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using TicketHub.Auth;

namespace Ingressos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructureIngressos(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IngressosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("IngressosDb"),
                sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        var servicosExternos = new ServicosExternosOptions();
        configuration.GetSection(ServicosExternosOptions.SectionName).Bind(servicosExternos);

        services.AdicionarClienteServicoInterno(configuration);

        services.AddHttpClient<IEventoExternalService, HttpEventoExternalService>(client =>
            {
                client.BaseAddress = new Uri(servicosExternos.EventosApiBaseUrl);
            })
            .AddHttpMessageHandler<AuthTokenDelegatingHandler>()
            .AddStandardResilienceHandler(ResilienciaHttpConfiguracao.Configurar);

        services.AddScoped<IIngressoRepositorio, IngressoRepositorio>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Ingressos.Application.Ingressos.Queries.ListarIngressosQuery>();
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.AddHostedService<LiberacaoReservaExpiradaWorker>();

        return services;
    }
}
