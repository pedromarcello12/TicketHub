using Eventos.Domain.Enums;
using TicketHub.Core.Entidades;

namespace Eventos.Domain.Entidades;

public class Evento : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Local { get; private set; } = string.Empty;
    public DateTime DataHora { get; private set; }
    public int CapacidadeTotal { get; private set; }
    public StatusEvento Status { get; private set; }

    private Evento() { }

    public Evento(string nome, string local, DateTime dataHora, int capacidadeTotal)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do evento é obrigatório.", nameof(nome));

        if (string.IsNullOrWhiteSpace(local))
            throw new ArgumentException("O local do evento é obrigatório.", nameof(local));

        if (dataHora <= DateTime.UtcNow)
            throw new ArgumentException("A data do evento precisa ser futura.", nameof(dataHora));

        if (capacidadeTotal <= 0)
            throw new ArgumentException("A capacidade total precisa ser maior que zero.", nameof(capacidadeTotal));

        Nome = nome;
        Local = local;
        DataHora = dataHora;
        CapacidadeTotal = capacidadeTotal;
        Status = StatusEvento.Planejado;
    }

    public void Publicar()
    {
        if (Status != StatusEvento.Planejado)
            throw new InvalidOperationException("Somente eventos planejados podem ser publicados.");

        Status = StatusEvento.Publicado;
    }

    public void Cancelar()
    {
        if (Status == StatusEvento.Encerrado)
            throw new InvalidOperationException("Eventos encerrados não podem ser cancelados.");

        Status = StatusEvento.Cancelado;
    }

    public void Encerrar()
    {
        if (Status != StatusEvento.Publicado)
            throw new InvalidOperationException("Somente eventos publicados podem ser encerrados.");

        Status = StatusEvento.Encerrado;
    }
}
