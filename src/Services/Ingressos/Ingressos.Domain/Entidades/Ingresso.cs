using Ingressos.Domain.Enums;
using TicketHub.Core.Entidades;

namespace Ingressos.Domain.Entidades;

public class Ingresso : EntidadeBase
{
    public Guid EventoId { get; private set; }
    public string TipoIngresso { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public StatusIngresso Status { get; private set; }

    private Ingresso() { }

    public Ingresso(Guid eventoId, string tipoIngresso, decimal preco)
    {
        if (eventoId == Guid.Empty)
            throw new ArgumentException("O ingresso precisa estar vinculado a um evento.", nameof(eventoId));

        if (string.IsNullOrWhiteSpace(tipoIngresso))
            throw new ArgumentException("O tipo do ingresso é obrigatório.", nameof(tipoIngresso));

        if (preco < 0)
            throw new ArgumentException("O preço não pode ser negativo.", nameof(preco));

        EventoId = eventoId;
        TipoIngresso = tipoIngresso;
        Preco = preco;
        Status = StatusIngresso.Disponivel;
    }

    public void Reservar()
    {
        if (Status != StatusIngresso.Disponivel)
            throw new InvalidOperationException("Somente ingressos disponíveis podem ser reservados.");

        Status = StatusIngresso.Reservado;
    }

    public void ConfirmarVenda()
    {
        if (Status != StatusIngresso.Reservado)
            throw new InvalidOperationException("Somente ingressos reservados podem ter a venda confirmada.");

        Status = StatusIngresso.Vendido;
    }

    public void Cancelar()
    {
        if (Status == StatusIngresso.Vendido)
            throw new InvalidOperationException("Ingressos já vendidos não podem ser cancelados.");

        Status = StatusIngresso.Cancelado;
    }
}
