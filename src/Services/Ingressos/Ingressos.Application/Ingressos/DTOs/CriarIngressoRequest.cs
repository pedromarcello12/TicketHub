namespace Ingressos.Application.Ingressos.DTOs;

public record CriarIngressoRequest(Guid EventoId, string TipoIngresso, decimal Preco);
