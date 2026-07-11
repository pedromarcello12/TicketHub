using Microsoft.EntityFrameworkCore;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Infrastructure.Persistencia;

public class PagamentosDbContext(DbContextOptions<PagamentosDbContext> options) : DbContext(options)
{
    public DbSet<EntidadePagamento> Pagamentos => Set<EntidadePagamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PagamentosDbContext).Assembly);
    }
}
