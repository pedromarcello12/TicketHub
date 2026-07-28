using MediatR;
using Pagamento.Application.Pagamentos.DTOs;
using Pagamento.Application.Pagamentos.Interfaces;
using TicketHub.Core.Excecoes;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Application.Pagamentos.Commands;

// ─── Criar ───────────────────────────────────────────────────────────────────

public record CriarPagamentoCommand(Guid IngressoId, decimal Valor, int Metodo, string EmailCliente)
    : IRequest<PagamentoResponse>;

public class CriarPagamentoCommandHandler(
    IPagamentoRepositorio repositorio,
    IPagamentoEventoPublisher eventoPublisher,
    IIngressoExternalService ingressoExternalService) : IRequestHandler<CriarPagamentoCommand, PagamentoResponse>
{
    public async Task<PagamentoResponse> Handle(CriarPagamentoCommand request, CancellationToken cancellationToken)
    {
        var ingressoExiste = await ingressoExternalService.ExisteAsync(request.IngressoId, cancellationToken);
        if (!ingressoExiste)
            throw new RecursoRelacionadoNaoEncontradoException($"Ingresso '{request.IngressoId}' não encontrado.");

        var pagamento = new EntidadePagamento(request.IngressoId, request.Valor, request.Metodo, request.EmailCliente);

        await repositorio.AdicionarAsync(pagamento, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return PagamentoResponse.DeEntidade(pagamento);
    }
}

// ─── Aprovar ─────────────────────────────────────────────────────────────────

public record AprovarPagamentoCommand(Guid Id) : IRequest<PagamentoResponse?>;

public class AprovarPagamentoCommandHandler(
    IPagamentoRepositorio repositorio,
    IPagamentoEventoPublisher eventoPublisher,
    INotificacaoRealTimeService notificacaoRealTime) : IRequestHandler<AprovarPagamentoCommand, PagamentoResponse?>
{
    public async Task<PagamentoResponse?> Handle(AprovarPagamentoCommand request, CancellationToken cancellationToken)
    {
        var pagamento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (pagamento is null) return null;

        pagamento.Aprovar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        // Publica no RabbitMQ (para e-mail via Notificacoes.Worker) e notifica via SignalR em tempo real
        await PublicarAsync(pagamento, eventoPublisher, cancellationToken);
        await notificacaoRealTime.NotificarStatusPagamentoAsync(
            pagamento.EmailCliente, pagamento.Id, pagamento.IngressoId,
            pagamento.Valor, pagamento.Status.ToString(), cancellationToken);

        return PagamentoResponse.DeEntidade(pagamento);
    }
}

// ─── Recusar ─────────────────────────────────────────────────────────────────

public record RecusarPagamentoCommand(Guid Id) : IRequest<PagamentoResponse?>;

public class RecusarPagamentoCommandHandler(
    IPagamentoRepositorio repositorio,
    IPagamentoEventoPublisher eventoPublisher,
    INotificacaoRealTimeService notificacaoRealTime) : IRequestHandler<RecusarPagamentoCommand, PagamentoResponse?>
{
    public async Task<PagamentoResponse?> Handle(RecusarPagamentoCommand request, CancellationToken cancellationToken)
    {
        var pagamento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (pagamento is null) return null;

        pagamento.Recusar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        await PublicarAsync(pagamento, eventoPublisher, cancellationToken);
        await notificacaoRealTime.NotificarStatusPagamentoAsync(
            pagamento.EmailCliente, pagamento.Id, pagamento.IngressoId,
            pagamento.Valor, pagamento.Status.ToString(), cancellationToken);

        return PagamentoResponse.DeEntidade(pagamento);
    }
}

// ─── Estornar ────────────────────────────────────────────────────────────────

public record EstornarPagamentoCommand(Guid Id) : IRequest<PagamentoResponse?>;

public class EstornarPagamentoCommandHandler(
    IPagamentoRepositorio repositorio,
    IPagamentoEventoPublisher eventoPublisher,
    INotificacaoRealTimeService notificacaoRealTime) : IRequestHandler<EstornarPagamentoCommand, PagamentoResponse?>
{
    public async Task<PagamentoResponse?> Handle(EstornarPagamentoCommand request, CancellationToken cancellationToken)
    {
        var pagamento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (pagamento is null) return null;

        pagamento.Estornar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        await PublicarAsync(pagamento, eventoPublisher, cancellationToken);
        await notificacaoRealTime.NotificarStatusPagamentoAsync(
            pagamento.EmailCliente, pagamento.Id, pagamento.IngressoId,
            pagamento.Valor, pagamento.Status.ToString(), cancellationToken);

        return PagamentoResponse.DeEntidade(pagamento);
    }
}

// ─── Helper ──────────────────────────────────────────────────────────────────

file static class PagamentoPublisherHelper
{
    internal static Task PublicarAsync(
        EntidadePagamento pagamento,
        IPagamentoEventoPublisher publisher,
        CancellationToken ct) =>
        publisher.PublicarStatusAlteradoAsync(
            pagamento.Id, pagamento.IngressoId, pagamento.Valor,
            pagamento.Status.ToString(), pagamento.EmailCliente, ct);
}
