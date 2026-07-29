using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;
using Eventos.Domain.Entidades;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Eventos.Application.Eventos.Commands;

public record CriarEventoCommand(string Nome, string Local, DateTime DataHora, int CapacidadeTotal)
    : IRequest<EventoResponse>;

public class CriarEventoCommandHandler(
    IEventoRepositorio repositorio,
    IDistributedCache cache) : IRequestHandler<CriarEventoCommand, EventoResponse>
{
    public async Task<EventoResponse> Handle(CriarEventoCommand request, CancellationToken cancellationToken)
    {
        var evento = new Evento(request.Nome, request.Local, request.DataHora, request.CapacidadeTotal);

        await repositorio.AdicionarAsync(evento, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        // Invalida lista em cache
        await cache.RemoveAsync("eventos:lista", cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }
}
