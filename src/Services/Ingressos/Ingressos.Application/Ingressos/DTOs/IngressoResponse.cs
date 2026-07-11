using Ingressos.Domain.Entidades;

namespace Ingressos.Application.Ingressos.DTOs;

public record IngressoResponse(
    Guid Id,
    Guid EventoId,
    string TipoIngresso,
    decimal Preco,
    string Status,
    DateTime? ReservadoAte)
{
    public static IngressoResponse DeEntidade(Ingresso ingresso) => new(
        ingresso.Id,
        ingresso.EventoId,
        ingresso.TipoIngresso,
        ingresso.Preco,
        ingresso.Status.ToString(),
        ingresso.ReservadoAte);
}
