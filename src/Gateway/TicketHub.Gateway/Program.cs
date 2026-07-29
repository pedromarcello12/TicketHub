using TicketHub.Auth;
using TicketHub.Observabilidade;

var builder = WebApplication.CreateBuilder(args);

builder.AdicionarObservabilidade("gateway");

// JWT — valida tokens nas rotas protegidas antes de fazer proxy
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);
builder.Services.AdicionarRateLimiting();
builder.Services.AdicionarCors(builder.Configuration);

// YARP — lê clusters e rotas do appsettings.json (seção ReverseProxy)
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services
    .AddHealthChecks()
    // Health checks passivos para cada serviço downstream
    .AddUrlGroup(new Uri("http://auth-api:8080/health"),       name: "auth-api",       tags: ["downstream"])
    .AddUrlGroup(new Uri("http://eventos-api:8080/health"),    name: "eventos-api",    tags: ["downstream"])
    .AddUrlGroup(new Uri("http://ingressos-api:8080/health"),  name: "ingressos-api",  tags: ["downstream"])
    .AddUrlGroup(new Uri("http://pagamento-api:8080/health"),  name: "pagamento-api",  tags: ["downstream"]);

var app = builder.Build();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Health check do próprio gateway + saúde dos serviços downstream
app.MapHealthChecks("/health");

// Todo o roteamento é feito pelo YARP com base no appsettings.json
app.MapReverseProxy();

app.Run();
