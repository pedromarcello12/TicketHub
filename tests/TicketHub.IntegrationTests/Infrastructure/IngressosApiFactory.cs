using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace TicketHub.IntegrationTests.Infrastructure;

public class IngressosApiFactory(SqlServerFixture sqlFixture) : WebApplicationFactory<Ingressos.Api.Program>
{
    /// <summary>
    /// Mock do serviço externo de eventos — permite controlar se o evento "existe" nos testes.
    /// </summary>
    public IEventoExternalService EventoExternalServiceMock { get; } =
        Substitute.For<IEventoExternalService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<IngressosDbContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            services.AddDbContext<IngressosDbContext>(options =>
                options.UseSqlServer(sqlFixture.GetConnectionStringPara("TicketHubIngressos_Test")));

            // Substitui a chamada HTTP ao Eventos.Api por um mock
            var svcDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IEventoExternalService));
            if (svcDescriptor is not null)
                services.Remove(svcDescriptor);

            services.AddSingleton(EventoExternalServiceMock);
        });

        builder.UseSetting("Jwt:SecretKey", "chave-de-teste-suficientemente-longa-para-256-bits");
        builder.UseSetting("Observabilidade:SeqUrl", "http://localhost:5341");
    }
}
