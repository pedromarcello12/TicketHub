using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notificacoes.Worker.Email;
using Notificacoes.Worker.Workers;
using NSubstitute;
using TicketHub.MessageBus;
using TicketHub.MessageBus.Eventos;
using Xunit;

namespace Notificacoes.Tests.Workers;

public class PagamentoStatusAlteradoConsumerTests
{
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly PagamentoStatusAlteradoConsumer _sut;

    public PagamentoStatusAlteradoConsumerTests()
    {
        var opcoes = Options.Create(new RabbitMqOptions());
        var logger = Substitute.For<ILogger<PagamentoStatusAlteradoConsumer>>();
        _sut = new PagamentoStatusAlteradoConsumer(opcoes, logger, _emailSender);
    }

    private Task TratarAsync(PagamentoStatusAlteradoEvent evento, CancellationToken cancellationToken = default)
    {
        var metodo = typeof(PagamentoStatusAlteradoConsumer).GetMethod(
            "TratarAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        return (Task)metodo.Invoke(_sut, [evento, cancellationToken])!;
    }

    [Fact]
    public async Task TratarAsync_DeveEnviarEmailParaOClienteDoEvento()
    {
        var evento = new PagamentoStatusAlteradoEvent
        {
            PagamentoId = Guid.NewGuid(),
            IngressoId = Guid.NewGuid(),
            Valor = 150m,
            Status = "Aprovado",
            EmailCliente = "cliente@exemplo.com"
        };

        await TratarAsync(evento);

        await _emailSender.Received(1).EnviarAsync(
            "cliente@exemplo.com",
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TratarAsync_DeveIncluirStatusNoAssunto()
    {
        var evento = new PagamentoStatusAlteradoEvent
        {
            PagamentoId = Guid.NewGuid(),
            IngressoId = Guid.NewGuid(),
            Valor = 99.9m,
            Status = "Recusado",
            EmailCliente = "cliente@exemplo.com"
        };

        await TratarAsync(evento);

        await _emailSender.Received(1).EnviarAsync(
            Arg.Any<string>(),
            Arg.Is<string>(assunto => assunto.Contains("Recusado")),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TratarAsync_DeveIncluirIdsDoPagamentoEIngressoNoCorpo()
    {
        var pagamentoId = Guid.NewGuid();
        var ingressoId = Guid.NewGuid();
        var evento = new PagamentoStatusAlteradoEvent
        {
            PagamentoId = pagamentoId,
            IngressoId = ingressoId,
            Valor = 200m,
            Status = "Aprovado",
            EmailCliente = "cliente@exemplo.com"
        };

        await TratarAsync(evento);

        await _emailSender.Received(1).EnviarAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>(corpo => corpo.Contains(pagamentoId.ToString()) && corpo.Contains(ingressoId.ToString())),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TratarAsync_PropagaExcecaoQuandoEnvioDeEmailFalha()
    {
        var evento = new PagamentoStatusAlteradoEvent
        {
            PagamentoId = Guid.NewGuid(),
            IngressoId = Guid.NewGuid(),
            Valor = 50m,
            Status = "Aprovado",
            EmailCliente = "cliente@exemplo.com"
        };

        _emailSender.EnviarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("SMTP indisponivel"));

        var acao = () => TratarAsync(evento);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }
}
