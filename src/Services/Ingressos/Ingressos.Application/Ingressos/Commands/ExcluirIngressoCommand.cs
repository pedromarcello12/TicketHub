using Ingressos.Application.Ingressos.Interfaces;
using MediatR;

namespace Ingressos.Application.Ingressos.Commands;

public record ExcluirIngressoCommand(Guid Id) : IRequest<bool>;

public class ExcluirIngressoCommandHandler(IIngressoRepositorio repositorio)
    : IRequestHandler<ExcluirIngressoCommand, bool>
{
    public async Task<bool> Handle(ExcluirIngressoCommand request, CancellationToken cancellationToken)
    {
        var ingresso = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (ingresso is null) return false;

        ingresso.Excluir();
        await repositorio.RemoverAsync(ingresso, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return true;
    }
}
