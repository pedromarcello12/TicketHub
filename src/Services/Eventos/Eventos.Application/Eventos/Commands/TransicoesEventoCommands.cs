using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Eventos.Application.Eventos.Commands;

// ─── Publicar ────────────────────────────────────────────────────────────────

public record PublicarEventoCommand(Guid Id) : IRequest<EventoResponse?>;

public class PublicarEventoCommandHandler(
    IEventoRepositorio repositorio,
    IDistributedCache cache) : IRequestHandler<PublicarEventoCommand, EventoResponse?>
{
    public async Task<EventoResponse?> Handle(PublicarEventoCommand request, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (evento is null) return null;

        evento.Publicar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);
        await InvalidarCacheAsync(cache, request.Id, cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }
}

// ─── Cancelar ────────────────────────────────────────────────────────────────

public record CancelarEventoCommand(Guid Id) : IRequest<EventoResponse?>;

public class CancelarEventoCommandHandler(
    IEventoRepositorio repositorio,
    IDistributedCache cache) : IRequestHandler<CancelarEventoCommand, EventoResponse?>
{
    public async Task<EventoResponse?> Handle(CancelarEventoCommand request, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (evento is null) return null;

        evento.Cancelar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);
        await InvalidarCacheAsync(cache, request.Id, cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }
}

// ─── Encerrar ────────────────────────────────────────────────────────────────

public record EncerrarEventoCommand(Guid Id) : IRequest<EventoResponse?>;

public class EncerrarEventoCommandHandler(
    IEventoRepositorio repositorio,
    IDistributedCache cache) : IRequestHandler<EncerrarEventoCommand, EventoResponse?>
{
    public async Task<EventoResponse?> Handle(EncerrarEventoCommand request, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (evento is null) return null;

        evento.Encerrar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);
        await InvalidarCacheAsync(cache, request.Id, cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }
}

// ─── Helper ──────────────────────────────────────────────────────────────────

file static class EventoCacheHelper
{
    internal static Task InvalidarCacheAsync(IDistributedCache cache, Guid id, CancellationToken ct) =>
        Task.WhenAll(
            cache.RemoveAsync($"evento:{id}", ct),
            cache.RemoveAsync("eventos:lista", ct));
}
