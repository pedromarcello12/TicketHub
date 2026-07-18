namespace TicketHub.MessageBus.Eventos;

public class PagamentoStatusAlteradoEvent : IntegrationEvent
{
    public Guid PagamentoId { get; init; }
    public Guid IngressoId { get; init; }
    public decimal Valor { get; init; }
    public string Status { get; init; } = string.Empty;
    public string EmailCliente { get; init; } = string.Empty;
}
