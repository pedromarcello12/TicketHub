using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventos.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var nome = typeof(TRequest).Name;

        logger.LogInformation("[MediatR] Iniciando {Request}", nome);
        var sw = Stopwatch.StartNew();

        var response = await next();

        sw.Stop();
        logger.LogInformation("[MediatR] {Request} concluído em {ElapsedMs}ms", nome, sw.ElapsedMilliseconds);

        return response;
    }
}
