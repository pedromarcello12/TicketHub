using TicketHub.MessageBus.Eventos;

namespace TicketHub.MessageBus;

public interface IEventoPublisher
{
    void Publicar<TEvento>(TEvento evento, string routingKey) where TEvento : IntegrationEvent;
}
