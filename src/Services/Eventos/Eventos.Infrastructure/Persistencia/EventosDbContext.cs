using Eventos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Infrastructure.Persistencia;

public class EventosDbContext(DbContextOptions<EventosDbContext> options) : DbContext(options)
{
    public DbSet<Evento> Eventos => Set<Evento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventosDbContext).Assembly);
    }
}
