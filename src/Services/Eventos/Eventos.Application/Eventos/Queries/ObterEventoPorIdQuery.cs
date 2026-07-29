using Eventos.Application.Behaviors;
using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;

namespace Eventos.Application.Eventos.Queries;

/// <summary>
/// Query cacheável por ID — cacheada individualmente por 5 minutos.
/// </summary>
public record ObterEventoPorIdQuery(Guid Id) : ICacheableQuery<EventoResponse?>
{
    public string CacheKey => $"evento:{Id}";
    public TimeSpan Expiracao => TimeSpan.FromMinutes(5);
}

public class ObterEventoPorIdQueryHandler(IEventoRepositorio repositorio)
    : MediatR.IRequestHandler<ObterEventoPorIdQuery, EventoResponse?>
{
    public async Task<EventoResponse?> Handle(ObterEventoPorIdQuery request, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return evento is null ? null : EventoResponse.DeEntidade(evento);
    }
}
