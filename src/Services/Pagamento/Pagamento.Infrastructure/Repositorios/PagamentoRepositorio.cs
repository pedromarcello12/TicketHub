using Microsoft.EntityFrameworkCore;
using Pagamento.Application.Pagamentos.Interfaces;
using Pagamento.Infrastructure.Persistencia;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Infrastructure.Repositorios;

public class PagamentoRepositorio(PagamentosDbContext dbContext) : IPagamentoRepositorio
{
    public async Task AdicionarAsync(EntidadePagamento pagamento, CancellationToken cancellationToken)
    {
        await dbContext.Pagamentos.AddAsync(pagamento, cancellationToken);
    }

    public async Task<EntidadePagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Pagamentos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<EntidadePagamento>> ListarAsync(Guid? ingressoId, CancellationToken cancellationToken)
    {
        var query = dbContext.Pagamentos.AsQueryable();

        if (ingressoId is not null)
            query = query.Where(p => p.IngressoId == ingressoId);

        return await query.OrderBy(p => p.CriadoEm).ToListAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
