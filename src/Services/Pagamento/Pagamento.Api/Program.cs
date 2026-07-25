using Pagamento.Api.Filtros;
using Pagamento.Infrastructure;
using TicketHub.Auth;
using TicketHub.Observabilidade;

var builder = WebApplication.CreateBuilder(args);

builder.AdicionarObservabilidade("pagamento-api");

// Adiciona os serviços ao container.

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
// Saiba mais sobre como configurar o OpenAPI em https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AdicionarInfrastructurePagamento(builder.Configuration);
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);
builder.Services.AdicionarRateLimiting();
builder.Services.AdicionarCors(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configura o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
