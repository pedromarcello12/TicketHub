using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Interfaces;
using MediatR;

namespace Ingressos.Application.Ingressos.Commands;

public record AtualizarIngressoCommand(Guid Id, string TipoIngresso, decimal Preco) : IRequest<IngressoResponse?>;

public class AtualizarIngressoCommandHandler(IIngressoRepositorio repositorio)
    : IRequestHandler<AtualizarIngressoCommand, IngressoResponse?>
{
    public async Task<IngressoResponse?> Handle(AtualizarIngressoCommand request, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (ingresso is null) return null;

        ingresso.Atualizar(request.TipoIngresso, request.Preco);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }
}
