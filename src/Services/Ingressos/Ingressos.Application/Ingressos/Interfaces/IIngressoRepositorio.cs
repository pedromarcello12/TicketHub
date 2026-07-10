using Ingressos.Domain.Entidades;

namespace Ingressos.Application.Ingressos.Interfaces;

public interface IIngressoRepositorio
{
    Task AdicionarAsync(Ingresso ingresso, CancellationToken cancellationToken);
    Task<Ingresso?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
