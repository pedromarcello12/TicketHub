using Eventos.Application.Eventos.Interfaces;
using Eventos.Domain.Entidades;
using Eventos.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Infrastructure.Repositorios;

public class EventoRepositorio(EventosDbContext dbContext) : IEventoRepositorio
{
    public async Task AdicionarAsync(Evento evento, CancellationToken cancellationToken)
    {
        await dbContext.Eventos.AddAsync(evento, cancellationToken);
    }

    public async Task<Evento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Eventos.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Evento>> ListarAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Eventos.OrderBy(e => e.DataHora).ToListAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
