using Eventos.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TicketHub.IntegrationTests.Infrastructure;

public class EventosApiFactory(SqlServerFixture sqlFixture) : WebApplicationFactory<Eventos.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<EventosDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<EventosDbContext>(options =>
                options.UseSqlServer(sqlFixture.GetConnectionStringPara("TicketHubEventos_Test")));
        });

        builder.UseSetting("Jwt:SecretKey", "chave-de-teste-suficientemente-longa-para-256-bits");
        builder.UseSetting("Observabilidade:SeqUrl", "http://localhost:5341");
    }
}
