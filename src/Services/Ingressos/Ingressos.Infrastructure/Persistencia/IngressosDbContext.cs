using Ingressos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Ingressos.Infrastructure.Persistencia;

public class IngressosDbContext(DbContextOptions<IngressosDbContext> options) : DbContext(options)
{
    public DbSet<Ingresso> Ingressos => Set<Ingresso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IngressosDbContext).Assembly);
    }
}
