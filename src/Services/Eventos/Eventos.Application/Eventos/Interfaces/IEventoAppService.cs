using Eventos.Application.Eventos.DTOs;

namespace Eventos.Application.Eventos.Interfaces;

public interface IEventoAppService
{
    Task<EventoResponse> CriarAsync(CriarEventoRequest request, CancellationToken cancellationToken);
    Task<EventoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<EventoResponse>> ListarAsync(CancellationToken cancellationToken);
    Task<EventoResponse?> PublicarAsync(Guid id, CancellationToken cancellationToken);
    Task<EventoResponse?> CancelarAsync(Guid id, CancellationToken cancellationToken);
    Task<EventoResponse?> EncerrarAsync(Guid id, CancellationToken cancellationToken);
}
