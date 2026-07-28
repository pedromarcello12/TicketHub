using Eventos.Application.Behaviors;
using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;

namespace Eventos.Application.Eventos.Queries;

/// <summary>
/// Query cacheável — o CachingBehavior serve o resultado do Redis automaticamente.
/// </summary>
public record ListarEventosQuery() : ICacheableQuery<IReadOnlyList<EventoResponse>>
{
    public string CacheKey => "eventos:lista";
    public TimeSpan Expiracao => TimeSpan.FromMinutes(5);
}

public class ListarEventosQueryHandler(IEventoRepositorio repositorio)
    : MediatR.IRequestHandler<ListarEventosQuery, IReadOnlyList<EventoResponse>>
{
    public async Task<IReadOnlyList<EventoResponse>> Handle(ListarEventosQuery request, CancellationToken cancellationToken)
    {
        var eventos = await repositorio.ListarAsync(cancellationToken);
        return eventos.Select(EventoResponse.DeEntidade).ToList();
    }
}
