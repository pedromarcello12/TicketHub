using Auth.Api.Filtros;
using Auth.Infrastructure;
using Auth.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketHub.Auth;
using TicketHub.Observabilidade;

var builder = WebApplication.CreateBuilder(args);

builder.AdicionarObservabilidade("auth-api");

// Adiciona os serviços ao container.

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
// Saiba mais sobre como configurar o OpenAPI em https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AdicionarEmissorJwt(builder.Configuration);
builder.Services.AdicionarInfrastructureAuth(builder.Configuration);
builder.Services.Configure<ServicoInternoOptions>(builder.Configuration.GetSection(ServicoInternoOptions.SectionName));
builder.Services.AdicionarRateLimiting();
builder.Services.AdicionarCors(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<Auth.Application.Auth.Interfaces.IPasswordHasher>();
    var servicoInterno = scope.ServiceProvider.GetRequiredService<IOptions<ServicoInternoOptions>>().Value;

    await dbContext.Database.MigrateAsync();
    await UsuariosSeeder.SemearAsync(dbContext, passwordHasher, servicoInterno, CancellationToken.None);
}

// Configura o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseCors();
app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
