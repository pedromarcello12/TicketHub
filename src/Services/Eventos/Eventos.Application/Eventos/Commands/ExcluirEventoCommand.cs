using Eventos.Application.Eventos.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Eventos.Application.Eventos.Commands;

public record ExcluirEventoCommand(Guid Id) : IRequest<bool>;

public class ExcluirEventoCommandHandler(
    IEventoRepositorio repositorio,
    IDistributedCache cache) : IRequestHandler<ExcluirEventoCommand, bool>
{
    public async Task<bool> Handle(ExcluirEventoCommand request, CancellationToken cancellationToken)
    {
        var evento = await repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (evento is null) return false;

        evento.Excluir();
        await repositorio.RemoverAsync(evento, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        await cache.RemoveAsync($"evento:{request.Id}", cancellationToken);
        await cache.RemoveAsync("eventos:lista", cancellationToken);

        return true;
    }
}
