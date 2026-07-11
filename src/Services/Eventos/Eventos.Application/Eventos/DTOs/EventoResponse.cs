using Eventos.Domain.Entidades;

namespace Eventos.Application.Eventos.DTOs;

public record EventoResponse(
    Guid Id,
    string Nome,
    string Local,
    DateTime DataHora,
    int CapacidadeTotal,
    string Status)
{
    public static EventoResponse DeEntidade(Evento evento) => new(
        evento.Id,
        evento.Nome,
        evento.Local,
        evento.DataHora,
        evento.CapacidadeTotal,
        evento.Status.ToString());
}
