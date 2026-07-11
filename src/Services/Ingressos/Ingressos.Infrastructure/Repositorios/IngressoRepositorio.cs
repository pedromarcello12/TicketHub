using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Domain.Entidades;
using Ingressos.Domain.Enums;
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

    public async Task<IReadOnlyList<Ingresso>> ListarAsync(Guid? eventoId, CancellationToken cancellationToken)
    {
        var query = dbContext.Ingressos.AsQueryable();

        if (eventoId is not null)
            query = query.Where(i => i.EventoId == eventoId);

        return await query.OrderBy(i => i.CriadoEm).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ingresso>> ListarReservasExpiradasAsync(DateTime agora, CancellationToken cancellationToken)
    {
        return await dbContext.Ingressos
            .Where(i => i.Status == StatusIngresso.Reservado && i.ReservadoAte != null && i.ReservadoAte <= agora)
            .ToListAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
