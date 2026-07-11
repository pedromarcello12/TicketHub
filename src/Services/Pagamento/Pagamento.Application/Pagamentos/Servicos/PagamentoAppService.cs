using Pagamento.Application.Pagamentos.DTOs;
using Pagamento.Application.Pagamentos.Interfaces;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Application.Pagamentos.Servicos;

public class PagamentoAppService(IPagamentoRepositorio repositorio) : IPagamentoAppService
{
    public async Task<PagamentoResponse> CriarAsync(CriarPagamentoRequest request, CancellationToken cancellationToken)
    {
        var pagamento = new EntidadePagamento(request.IngressoId, request.Valor, request.Metodo);

        await repositorio.AdicionarAsync(pagamento, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return PagamentoResponse.DeEntidade(pagamento);
    }

    public async Task<PagamentoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await repositorio.ObterPorIdAsync(id, cancellationToken);

        return pagamento is null ? null : PagamentoResponse.DeEntidade(pagamento);
    }

    public async Task<IReadOnlyList<PagamentoResponse>> ListarAsync(Guid? ingressoId, CancellationToken cancellationToken)
    {
        var pagamentos = await repositorio.ListarAsync(ingressoId, cancellationToken);

        return pagamentos.Select(PagamentoResponse.DeEntidade).ToList();
    }

    public async Task<PagamentoResponse?> AprovarAsync(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (pagamento is null)
            return null;

        pagamento.Aprovar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return PagamentoResponse.DeEntidade(pagamento);
    }

    public async Task<PagamentoResponse?> RecusarAsync(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (pagamento is null)
            return null;

        pagamento.Recusar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return PagamentoResponse.DeEntidade(pagamento);
    }

    public async Task<PagamentoResponse?> EstornarAsync(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (pagamento is null)
            return null;

        pagamento.Estornar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return PagamentoResponse.DeEntidade(pagamento);
    }
}
