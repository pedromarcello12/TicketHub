using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pagamento.Application.Behaviors;
using Pagamento.Application.Pagamentos.Interfaces;
using Pagamento.Infrastructure.Integracao;
using Pagamento.Infrastructure.Persistencia;
using Pagamento.Infrastructure.Repositorios;
using Pagamento.Infrastructure.ServicosExternos;
using TicketHub.Auth;
using TicketHub.MessageBus;

namespace Pagamento.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructurePagamento(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PagamentosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("PagamentoDb"),
                sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        // MassTransit — somente publisher, sem consumers neste serviço
        services.AdicionarMassTransitRabbitMq(configuration);

        var servicosExternos = new ServicosExternosOptions();
        configuration.GetSection(ServicosExternosOptions.SectionName).Bind(servicosExternos);

        services.AdicionarClienteServicoInterno(configuration);

        services.AddHttpClient<IIngressoExternalService, HttpIngressoExternalService>(client =>
            {
                client.BaseAddress = new Uri(servicosExternos.IngressosApiBaseUrl);
            })
            .AddHttpMessageHandler<AuthTokenDelegatingHandler>()
            .AddStandardResilienceHandler(ResilienciaHttpConfiguracao.Configurar);

        services.AddScoped<IPagamentoRepositorio, PagamentoRepositorio>();
        services.AddScoped<IPagamentoEventoPublisher, PagamentoEventoPublisher>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Pagamento.Application.Pagamentos.Queries.ListarPagamentosQuery>();
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        return services;
    }
}
