using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Interfaces;
using MediatR;

namespace Ingressos.Application.Ingressos.Commands;

// ─── Reservar ────────────────────────────────────────────────────────────────

public record ReservarIngressoCommand(Guid Id) : IRequest<IngressoResponse?>;

public class ReservarIngressoCommandHandler(IIngressoRepositorio repositorio)
    : IRequestHandler<ReservarIngressoCommand, IngressoResponse?>
{
    public async Task<IngressoResponse?> Handle(ReservarIngressoCommand request, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (ingresso is null) return null;

        ingresso.Reservar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }
}

// ─── ConfirmarVenda ───────────────────────────────────────────────────────────

public record ConfirmarVendaIngressoCommand(Guid Id) : IRequest<IngressoResponse?>;

public class ConfirmarVendaIngressoCommandHandler(IIngressoRepositorio repositorio)
    : IRequestHandler<ConfirmarVendaIngressoCommand, IngressoResponse?>
{
    public async Task<IngressoResponse?> Handle(ConfirmarVendaIngressoCommand request, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (ingresso is null) return null;

        ingresso.ConfirmarVenda();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }
}

// ─── Cancelar ────────────────────────────────────────────────────────────────

public record CancelarIngressoCommand(Guid Id) : IRequest<IngressoResponse?>;

public class CancelarIngressoCommandHandler(IIngressoRepositorio repositorio)
    : IRequestHandler<CancelarIngressoCommand, IngressoResponse?>
{
    public async Task<IngressoResponse?> Handle(CancelarIngressoCommand request, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (ingresso is null) return null;

        ingresso.Cancelar();
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }
}

// ─── LiberarReservasExpiradas (usado pelo BackgroundService) ─────────────────

public record LiberarReservasExpiradasCommand : IRequest<int>;

public class LiberarReservasExpiradasCommandHandler(IIngressoRepositorio repositorio)
    : IRequestHandler<LiberarReservasExpiradasCommand, int>
{
    public async Task<int> Handle(LiberarReservasExpiradasCommand request, CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;
        var expirados = await repositorio.ListarReservasExpiradasAsync(agora, cancellationToken);

        if (expirados.Count == 0) return 0;

        foreach (var ingresso in expirados)
            ingresso.LiberarReservaExpirada(agora);

        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return expirados.Count;
    }
}
