using Pagamento.Domain.Enums;
using TicketHub.Core.Entidades;

namespace Pagamento.Domain.Entidades;

public class Pagamento : EntidadeBase
{
    public Guid IngressoId { get; private set; }
    public decimal Valor { get; private set; }
    public MetodoPagamento Metodo { get; private set; }
    public StatusPagamento Status { get; private set; }
    public string EmailCliente { get; private set; } = string.Empty;

    private Pagamento() { }

    public Pagamento(Guid ingressoId, decimal valor, MetodoPagamento metodo, string emailCliente)
    {
        if (ingressoId == Guid.Empty)
            throw new ArgumentException("O pagamento precisa estar vinculado a um ingresso.", nameof(ingressoId));

        if (valor <= 0)
            throw new ArgumentException("O valor do pagamento precisa ser maior que zero.", nameof(valor));

        if (string.IsNullOrWhiteSpace(emailCliente) || !emailCliente.Contains('@'))
            throw new ArgumentException("O email do cliente e obrigatorio e precisa ser valido.", nameof(emailCliente));

        IngressoId = ingressoId;
        Valor = valor;
        Metodo = metodo;
        EmailCliente = emailCliente;
        Status = StatusPagamento.Pendente;
    }

    public void Aprovar()
    {
        if (Status != StatusPagamento.Pendente)
            throw new InvalidOperationException("Somente pagamentos pendentes podem ser aprovados.");

        Status = StatusPagamento.Aprovado;
    }

    public void Recusar()
    {
        if (Status != StatusPagamento.Pendente)
            throw new InvalidOperationException("Somente pagamentos pendentes podem ser recusados.");

        Status = StatusPagamento.Recusado;
    }

    public void Estornar()
    {
        if (Status != StatusPagamento.Aprovado)
            throw new InvalidOperationException("Somente pagamentos aprovados podem ser estornados.");

        Status = StatusPagamento.Estornado;
    }
}
