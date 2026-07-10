using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Domain.Entidades;
using Ingressos.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Ingressos.Infrastructure.Repositorios;

public class IngressoRepositorio(IngressosDbContext dbContext) : IIngressoRepositorio
{
    public async Task AdicionarAsync(Ingresso ingresso, CancellationToken cancellationToken)
    {
        await dbContext.Ingressos.AddAsync(ingresso, cancellationToken);
    }

    public async Task<Ingresso?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Ingressos.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
