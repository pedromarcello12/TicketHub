using Auth.Api.Filtros;
using Auth.Infrastructure;
using Auth.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using TicketHub.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AdicionarEmissorJwt(builder.Configuration);
builder.Services.AdicionarInfrastructureAuth(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<Auth.Application.Auth.Interfaces.IPasswordHasher>();

    await dbContext.Database.MigrateAsync();
    await UsuariosSeeder.SemearAsync(dbContext, passwordHasher, CancellationToken.None);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
