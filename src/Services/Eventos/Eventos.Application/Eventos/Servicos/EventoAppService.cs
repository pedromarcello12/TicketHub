using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;
using Eventos.Domain.Entidades;

namespace Eventos.Application.Eventos.Servicos;

public class EventoAppService(IEventoRepositorio repositorio) : IEventoAppService
{
    public async Task<EventoResponse> CriarAsync(CriarEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = new Evento(request.Nome, request.Local, request.DataHora, request.CapacidadeTotal);

        await repositorio.AdicionarAsync(evento, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }

    public async Task<EventoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(id, cancellationToken);

        return evento is null ? null : EventoResponse.DeEntidade(evento);
    }

    public async Task<IReadOnlyList<EventoResponse>> ListarAsync(CancellationToken cancellationToken)
    {
        var eventos = await repositorio.ListarAsync(cancellationToken);

        return eventos.Select(EventoResponse.DeEntidade).ToList();
    }

    public async Task<EventoResponse?> PublicarAsync(Guid id, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (evento is null)
            return null;

        evento.Publicar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }

    public async Task<EventoResponse?> CancelarAsync(Guid id, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (evento is null)
            return null;

        evento.Cancelar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }

    public async Task<EventoResponse?> EncerrarAsync(Guid id, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (evento is null)
            return null;

        evento.Encerrar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return EventoResponse.DeEntidade(evento);
    }
}
