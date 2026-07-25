using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace TicketHub.Observabilidade;

public static class DependencyInjection
{
    /// <summary>
    /// Configura Serilog para aplicações ASP.NET Core (WebApplicationBuilder).
    /// Deve ser chamado antes de builder.Build().
    /// </summary>
    public static WebApplicationBuilder AdicionarObservabilidade(
        this WebApplicationBuilder builder,
        string nomeServico)
    {
        var opcoes = ObterOpcoes(builder.Configuration);

        ConfigurarSerilog(builder.Host, builder.Configuration, nomeServico, opcoes);
        ConfigurarOpenTelemetry(builder.Services, builder.Configuration, nomeServico, opcoes, isWorker: false);

        return builder;
    }

    /// <summary>
    /// Configura Serilog para Worker Services (HostApplicationBuilder).
    /// Deve ser chamado antes de builder.Build().
    /// </summary>
    public static HostApplicationBuilder AdicionarObservabilidadeWorker(
        this HostApplicationBuilder builder,
        string nomeServico)
    {
        var opcoes = ObterOpcoes(builder.Configuration);

        ConfigurarSerilogWorker(builder.Logging, builder.Configuration, nomeServico, opcoes);
        ConfigurarOpenTelemetry(builder.Services, builder.Configuration, nomeServico, opcoes, isWorker: true);

        return builder;
    }

    private static void ConfigurarSerilog(
        IHostBuilder host,
        IConfiguration configuration,
        string nomeServico,
        ObservabilidadeOptions opcoes)
    {
        host.UseSerilog((ctx, services, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Servico", nomeServico)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servico}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Seq(
                    serverUrl: opcoes.SeqUrl,
                    restrictedToMinimumLevel: LogEventLevel.Information);
        });
    }

    private static void ConfigurarSerilogWorker(
        ILoggingBuilder logging,
        IConfiguration configuration,
        string nomeServico,
        ObservabilidadeOptions opcoes)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Servico", nomeServico)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servico}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(
                serverUrl: opcoes.SeqUrl,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

        Log.Logger = logger;
        logging.ClearProviders();
        logging.AddSerilog(logger);
    }

    private static void ConfigurarOpenTelemetry(
        IServiceCollection services,
        IConfiguration configuration,
        string nomeServico,
        ObservabilidadeOptions opcoes,
        bool isWorker)
    {
        var versao = typeof(DependencyInjection).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        var otelBuilder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: nomeServico,
                    serviceVersion: versao)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"]
                        ?? configuration["DOTNET_ENVIRONMENT"]
                        ?? "Production"
                }));

        otelBuilder.WithTracing(tracing =>
        {
            if (!isWorker)
                tracing.AddAspNetCoreInstrumentation(opt =>
                {
                    opt.RecordException = true;
                    opt.Filter = ctx =>
                        !ctx.Request.Path.StartsWithSegments("/health") &&
                        !ctx.Request.Path.StartsWithSegments("/openapi");
                });

            tracing
                .AddHttpClientInstrumentation(opt => opt.RecordException = true)
                .AddOtlpExporter(opt => opt.Endpoint = new Uri(opcoes.OtlpEndpoint));

            if (opcoes.HabilitarConsoleExporter)
                tracing.AddConsoleExporter();
        });

        otelBuilder.WithMetrics(metrics =>
        {
            if (!isWorker)
                metrics.AddAspNetCoreInstrumentation();

            metrics
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opt => opt.Endpoint = new Uri(opcoes.OtlpEndpoint));
        });
    }

    private static ObservabilidadeOptions ObterOpcoes(IConfiguration configuration)
    {
        var opcoes = new ObservabilidadeOptions();
        configuration.GetSection(ObservabilidadeOptions.SectionName).Bind(opcoes);
        return opcoes;
    }
}
