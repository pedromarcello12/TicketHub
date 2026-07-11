namespace TicketHub.MessageBus.Eventos;

public abstract class IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OcorridoEm { get; init; } = DateTime.UtcNow;
}
