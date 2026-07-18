using Pagamento.Api.Filtros;
using Pagamento.Infrastructure;
using Pagamento.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using TicketHub.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AdicionarInfrastructurePagamento(builder.Configuration);
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);
builder.Services.AdicionarRateLimiting();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PagamentosDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
