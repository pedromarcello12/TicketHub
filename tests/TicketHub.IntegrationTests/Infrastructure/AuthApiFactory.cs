using Auth.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TicketHub.IntegrationTests.Infrastructure;

public class AuthApiFactory(SqlServerFixture sqlFixture) : WebApplicationFactory<Auth.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Substitui o DbContext pelo banco de teste
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(sqlFixture.GetConnectionStringPara("TicketHubAuth_Test")));
        });

        builder.UseSetting("Jwt:SecretKey", "chave-de-teste-suficientemente-longa-para-256-bits");
        builder.UseSetting("ServicoInterno:Senha", "senha-interna-de-teste");
        builder.UseSetting("Observabilidade:SeqUrl", "http://localhost:5341");
    }
}
