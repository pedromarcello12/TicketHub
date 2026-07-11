namespace TicketHub.Core.Excecoes;

public class ServicoExternoIndisponivelException(string mensagem, Exception? causa = null)
    : Exception(mensagem, causa);
