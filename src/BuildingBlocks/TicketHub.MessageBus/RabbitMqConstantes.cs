namespace TicketHub.MessageBus;

public static class RabbitMqConstantes
{
    public const string ExchangeEventos = "tickethub.eventos";

    public static class RoutingKeys
    {
        public const string PagamentoStatusAlterado = "pagamento.status-alterado";
    }

    public static class Filas
    {
        public const string NotificacoesPagamentoStatusAlterado = "notificacoes.pagamento-status-alterado";
    }
}
