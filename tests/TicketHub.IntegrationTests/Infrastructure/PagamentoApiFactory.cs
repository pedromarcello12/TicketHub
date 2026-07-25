using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pagamento.Application.Pagamentos.Interfaces;
using Pagamento.Infrastructure.Persistencia;

namespace TicketHub.IntegrationTests.Infrastructure;

public class PagamentoApiFactory(SqlServerFixture sqlFixture) : WebApplicationFactory<Pagamento.Api.Program>
{
    public IIngressoExternalService IngressoExternalServiceMock { get; } =
        Substitute.For<IIngressoExternalService>();

    public IPagamentoEventoPublisher EventoPublisherMock { get; } =
        Substitute.For<IPagamentoEventoPublisher>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<PagamentosDbContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            services.AddDbContext<PagamentosDbContext>(options =>
                options.UseSqlServer(sqlFixture.GetConnectionStringPara("TicketHubPagamento_Test")));

            // Mock das dependências externas
            RemoverESubstituir<IIngressoExternalService>(services, IngressoExternalServiceMock);
            RemoverESubstituir<IPagamentoEventoPublisher>(services, EventoPublisherMock);
        });

        builder.UseSetting("Jwt:SecretKey", "chave-de-teste-suficientemente-longa-para-256-bits");
        builder.UseSetting("Observabilidade:SeqUrl", "http://localhost:5341");
    }

    private static void RemoverESubstituir<T>(IServiceCollection services, T substituto) where T : class
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor is not null)
            services.Remove(descriptor);
        services.AddSingleton(substituto);
    }
}
