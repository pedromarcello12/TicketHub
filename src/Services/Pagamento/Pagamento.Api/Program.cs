using Microsoft.AspNetCore.SignalR;
using Pagamento.Api.Filtros;
using Pagamento.Api.Hubs;
using Pagamento.Application.Pagamentos.Interfaces;
using Pagamento.Infrastructure;
using TicketHub.Auth;
using TicketHub.Observabilidade;

var builder = WebApplication.CreateBuilder(args);

builder.AdicionarObservabilidade("pagamento-api");

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
builder.Services.AddOpenApi();

builder.Services.AdicionarInfrastructurePagamento(builder.Configuration);
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);
builder.Services.AdicionarRateLimiting();
builder.Services.AdicionarCors(builder.Configuration);
builder.Services.AddHealthChecks();

// SignalR — usa nomeUsuario como identificador de usuário para grupos de notificação
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NomeUsuarioIdProvider>();
builder.Services.AddScoped<INotificacaoRealTimeService, SignalRNotificacaoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

if (app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

// Hub de notificações em tempo real
app.MapHub<NotificacoesHub>("/hubs/notificacoes");

app.Run();
