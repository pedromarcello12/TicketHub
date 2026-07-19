using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Application.Ingressos.Servicos;
using Ingressos.Infrastructure.Cache;
using Ingressos.Infrastructure.Jobs;
using Ingressos.Infrastructure.Persistencia;
using Ingressos.Infrastructure.Repositorios;
using Ingressos.Infrastructure.ServicosExternos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using StackExchange.Redis;
using TicketHub.Auth;

namespace Ingressos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructureIngressos(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IngressosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("IngressosDb")));

        var servicosExternos = new ServicosExternosOptions();
        configuration.GetSection(ServicosExternosOptions.SectionName).Bind(servicosExternos);

        services.AdicionarClienteServicoInterno(configuration);

        services.AddHttpClient<IEventoExternalService, HttpEventoExternalService>(client =>
            {
                client.BaseAddress = new Uri(servicosExternos.EventosApiBaseUrl);
            })
            .AddHttpMessageHandler<AuthTokenDelegatingHandler>()
            .AddStandardResilienceHandler(ResilienciaHttpConfiguracao.Configurar);

        var redisOptions = new RedisOptions();
        configuration.GetSection(RedisOptions.SectionName).Bind(redisOptions);

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

        services.AddScoped<IIngressoRepositorio, IngressoRepositorio>();
        services.AddScoped<IIngressoAppService, IngressoAppService>();
        services.AddHostedService<LiberacaoReservaExpiradaWorker>();

        return services;
    }
}
