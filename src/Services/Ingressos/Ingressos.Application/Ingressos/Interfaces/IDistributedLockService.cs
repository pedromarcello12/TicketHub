namespace Ingressos.Application.Ingressos.Interfaces;

public interface IDistributedLockService
{
    Task<IAsyncDisposable?> AdquirirAsync(string chave, TimeSpan duracao, CancellationToken cancellationToken);
}
