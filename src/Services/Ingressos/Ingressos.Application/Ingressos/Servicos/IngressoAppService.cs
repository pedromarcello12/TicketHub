using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Domain.Entidades;
using TicketHub.Core.Excecoes;

namespace Ingressos.Application.Ingressos.Servicos;

public class IngressoAppService(
    IIngressoRepositorio repositorio,
    IEventoExternalService eventoExternalService,
    IDistributedLockService lockService) : IIngressoAppService
{
    private static readonly TimeSpan DuracaoLockReserva = TimeSpan.FromSeconds(5);

    public async Task<IngressoResponse> CriarAsync(CriarIngressoRequest request, CancellationToken cancellationToken)
    {
        var eventoExiste = await eventoExternalService.ExisteAsync(request.EventoId, cancellationToken);
        if (!eventoExiste)
            throw new RecursoRelacionadoNaoEncontradoException($"Evento '{request.EventoId}' não encontrado.");

        var ingresso = new Ingresso(request.EventoId, request.TipoIngresso, request.Preco);

        await repositorio.AdicionarAsync(ingresso, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }

    public async Task<IngressoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(id, cancellationToken);

        return ingresso is null ? null : IngressoResponse.DeEntidade(ingresso);
    }

    public async Task<IReadOnlyList<IngressoResponse>> ListarAsync(Guid? eventoId, CancellationToken cancellationToken)
    {
        var ingressos = await repositorio.ListarAsync(eventoId, cancellationToken);

        return ingressos.Select(IngressoResponse.DeEntidade).ToList();
    }

    public async Task<IngressoResponse?> ReservarAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var lockHandle = await lockService.AdquirirAsync($"ingresso-reserva:{id}", DuracaoLockReserva, cancellationToken);
        if (lockHandle is null)
            throw new InvalidOperationException("O ingresso está sendo processado por outra requisição, tente novamente em instantes.");

        var ingresso = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (ingresso is null)
            return null;

        ingresso.Reservar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }

    public async Task<IngressoResponse?> ConfirmarVendaAsync(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (ingresso is null)
            return null;

        ingresso.ConfirmarVenda();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }

    public async Task<IngressoResponse?> CancelarAsync(Guid id, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(id, cancellationToken);
        if (ingresso is null)
            return null;

        ingresso.Cancelar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }

    public async Task<int> LiberarReservasExpiradasAsync(CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;
        var expirados = await repositorio.ListarReservasExpiradasAsync(agora, cancellationToken);

        if (expirados.Count == 0)
            return 0;

        foreach (var ingresso in expirados)
            ingresso.LiberarReservaExpirada(agora);

        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return expirados.Count;
    }
}
