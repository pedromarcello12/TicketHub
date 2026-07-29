using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Eventos.Application.Eventos.Commands;

public record AtualizarEventoCommand(Guid Id, string Nome, string Local, DateTime DataHora, int CapacidadeTotal)
    : IRequest<EventoResponse?>;

public class AtualizarEventoCommandHandler(
    IEventoRepositorio repositorio,
    IDistributedCache cache) : IRequestHandler<AtualizarEventoCommand, EventoResponse?>
{
    public async Task<EventoResponse?> Handle(AtualizarEventoCommand request, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (evento is null) return null;

        evento.Atualizar(request.Nome, request.Local, request.DataHora, request.CapacidadeTotal);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        await cache.RemoveAsync($"evento:{request.Id}", cancellationToken);
        await cache.RemoveAsync("eventos:lista", cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }
}
