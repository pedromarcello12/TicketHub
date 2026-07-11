using Eventos.Domain.Entidades;

namespace Eventos.Application.Eventos.Interfaces;

public interface IEventoRepositorio
{
    Task AdicionarAsync(Evento evento, CancellationToken cancellationToken);
    Task<Evento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Evento>> ListarAsync(CancellationToken cancellationToken);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
