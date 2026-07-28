using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Domain.Entidades;
using MediatR;
using TicketHub.Core.Excecoes;

namespace Ingressos.Application.Ingressos.Commands;

public record CriarIngressoCommand(Guid EventoId, string TipoIngresso, decimal Preco) : IRequest<IngressoResponse>;

public class CriarIngressoCommandHandler(
    IIngressoRepositorio repositorio,
    IEventoExternalService eventoExternalService) : IRequestHandler<CriarIngressoCommand, IngressoResponse>
{
    public async Task<IngressoResponse> Handle(CriarIngressoCommand request, CancellationToken cancellationToken)
    {
        var eventoExiste = await eventoExternalService.ExisteAsync(request.EventoId, cancellationToken);
        if (!eventoExiste)
            throw new RecursoRelacionadoNaoEncontradoException($"Evento '{request.EventoId}' não encontrado.");

        var ingresso = new Ingresso(request.EventoId, request.TipoIngresso, request.Preco);

        await repositorio.AdicionarAsync(ingresso, cancellationToken);
        await repositorio.SalvarAlteracoesAsync(cancellationToken);

        return IngressoResponse.DeEntidade(ingresso);
    }
}
