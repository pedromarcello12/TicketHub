namespace Eventos.Application.Eventos.DTOs;

public record CriarEventoRequest(string Nome, string Local, DateTime DataHora, int CapacidadeTotal);
