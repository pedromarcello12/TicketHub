using Auth.Api.Filtros;
using Auth.Infrastructure;
using Auth.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketHub.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AdicionarEmissorJwt(builder.Configuration);
builder.Services.AdicionarInfrastructureAuth(builder.Configuration);
builder.Services.Configure<ServicoInternoOptions>(builder.Configuration.GetSection(ServicoInternoOptions.SectionName));
builder.Services.AdicionarRateLimiting();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<Auth.Application.Auth.Interfaces.IPasswordHasher>();
    var servicoInterno = scope.ServiceProvider.GetRequiredService<IOptions<ServicoInternoOptions>>().Value;

    await dbContext.Database.MigrateAsync();
    await UsuariosSeeder.SemearAsync(dbContext, passwordHasher, servicoInterno, CancellationToken.None);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.MapControllers();

app.Run();
