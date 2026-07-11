namespace Ingressos.Application.Ingressos.Interfaces;

public interface IEventoExternalService
{
    Task<bool> ExisteAsync(Guid eventoId, CancellationToken cancellationToken);
}
