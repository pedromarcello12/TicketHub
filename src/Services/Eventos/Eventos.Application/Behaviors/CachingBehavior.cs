using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Eventos.Application.Behaviors;

/// <summary>
/// Pipeline behavior que intercepta queries marcadas com ICacheableQuery
/// e serve o resultado do cache distribuído, evitando hits desnecessários ao banco.
/// </summary>
public class CachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var chave = request.CacheKey;
        var cached = await cache.GetStringAsync(chave, cancellationToken);

        if (cached is not null)
        {
            logger.LogInformation("[Cache] Hit: {CacheKey}", chave);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        logger.LogInformation("[Cache] Miss: {CacheKey}", chave);
        var response = await next();

        var opcoes = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = request.Expiracao
        };

        await cache.SetStringAsync(chave, JsonSerializer.Serialize(response), opcoes, cancellationToken);

        return response;
    }
}
