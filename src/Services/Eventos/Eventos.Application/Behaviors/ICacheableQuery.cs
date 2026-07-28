using MediatR;

namespace Eventos.Application.Behaviors;

/// <summary>
/// Marca uma Query para ser cacheada automaticamente pelo CachingBehavior.
/// </summary>
public interface ICacheableQuery<TResponse> : IRequest<TResponse>
{
    string CacheKey { get; }
    TimeSpan Expiracao { get; }
}
