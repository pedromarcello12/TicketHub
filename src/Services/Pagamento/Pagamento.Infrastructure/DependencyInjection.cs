using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pagamento.Application.Pagamentos.Interfaces;
using Pagamento.Application.Pagamentos.Servicos;
using Pagamento.Infrastructure.Integracao;
using Pagamento.Infrastructure.Persistencia;
using Pagamento.Infrastructure.Repositorios;
using Pagamento.Infrastructure.ServicosExternos;
using TicketHub.MessageBus;

namespace Pagamento.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructurePagamento(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PagamentosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("PagamentoDb")));

        services.AdicionarRabbitMq(configuration);

        var servicosExternos = new ServicosExternosOptions();
        configuration.GetSection(ServicosExternosOptions.SectionName).Bind(servicosExternos);

        services.AddHttpClient<IIngressoExternalService, HttpIngressoExternalService>(client =>
            {
                client.BaseAddress = new Uri(servicosExternos.IngressosApiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .AddStandardResilienceHandler();

        services.AddScoped<IPagamentoRepositorio, PagamentoRepositorio>();
        services.AddScoped<IPagamentoEventoPublisher, PagamentoEventoPublisher>();
        services.AddScoped<IPagamentoAppService, PagamentoAppService>();

        return services;
    }
}
