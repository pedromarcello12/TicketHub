using System.Text.Json;
using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Eventos.Infrastructure.Cache;

public class CachedEventoAppService(
    IEventoAppService inner,
    IDistributedCache cache) : IEventoAppService
{
    private static readonly DistributedCacheEntryOptions _opcoesPadrao = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private static string ChaveEvento(Guid id) => $"evento:{id}";
    private const string ChaveLista = "eventos:lista";

    // ── Leituras com cache ──────────────────────────────────────────

    public async Task<EventoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var chave = ChaveEvento(id);
        var cached = await cache.GetStringAsync(chave, cancellationToken);

        if (cached is not null)
            return JsonSerializer.Deserialize<EventoResponse>(cached);

        var resultado = await inner.ObterPorIdAsync(id, cancellationToken);

        if (resultado is not null)
            await cache.SetStringAsync(chave, JsonSerializer.Serialize(resultado), _opcoesPadrao, cancellationToken);

        return resultado;
    }

    public async Task<IReadOnlyList<EventoResponse>> ListarAsync(CancellationToken cancellationToken)
    {
        var cached = await cache.GetStringAsync(ChaveLista, cancellationToken);

        if (cached is not null)
            return JsonSerializer.Deserialize<List<EventoResponse>>(cached)!;

        var resultado = await inner.ListarAsync(cancellationToken);

        await cache.SetStringAsync(ChaveLista, JsonSerializer.Serialize(resultado), _opcoesPadrao, cancellationToken);

        return resultado;
    }

    // ── Escritas com invalidação ────────────────────────────────────

    public async Task<EventoResponse> CriarAsync(CriarEventoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await inner.CriarAsync(request, cancellationToken);
        await cache.RemoveAsync(ChaveLista, cancellationToken);
        return resultado;
    }

    public async Task<EventoResponse?> PublicarAsync(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await inner.PublicarAsync(id, cancellationToken);
        await InvalidarEventoAsync(id, cancellationToken);
        return resultado;
    }

    public async Task<EventoResponse?> CancelarAsync(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await inner.CancelarAsync(id, cancellationToken);
        await InvalidarEventoAsync(id, cancellationToken);
        return resultado;
    }

    public async Task<EventoResponse?> EncerrarAsync(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await inner.EncerrarAsync(id, cancellationToken);
        await InvalidarEventoAsync(id, cancellationToken);
        return resultado;
    }

    // ── Helpers ────────────────────────────────────────────────────

    private async Task InvalidarEventoAsync(Guid id, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(ChaveEvento(id), cancellationToken);
        await cache.RemoveAsync(ChaveLista, cancellationToken);
    }
}
