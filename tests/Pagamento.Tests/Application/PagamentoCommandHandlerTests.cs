using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Pagamento.Application.Pagamentos.Commands;
using Pagamento.Application.Pagamentos.Interfaces;
using Pagamento.Domain.Enums;
using TicketHub.Core.Excecoes;
using Xunit;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Tests.Application;

/// <summary>
/// Testes unitários para os command handlers de Pagamento (padrão CQRS + MediatR).
/// Todas as dependências são mockadas com NSubstitute.
/// </summary>

// ─── CriarPagamentoCommandHandler ────────────────────────────────────────────

public class CriarPagamentoCommandHandlerTests
{
    private readonly IPagamentoRepositorio _repositorio = Substitute.For<IPagamentoRepositorio>();
    private readonly IPagamentoEventoPublisher _eventoPublisher = Substitute.For<IPagamentoEventoPublisher>();
    private readonly IIngressoExternalService _ingressoService = Substitute.For<IIngressoExternalService>();
    private readonly CriarPagamentoCommandHandler _handler;

    public CriarPagamentoCommandHandlerTests()
        => _handler = new CriarPagamentoCommandHandler(_repositorio, _eventoPublisher, _ingressoService);

    [Fact]
    public async Task Handle_QuandoIngressoExiste_DeveCriarESalvarComStatusPendente()
    {
        var ingressoId = Guid.NewGuid();
        _ingressoService.ExisteAsync(ingressoId, Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await _handler.Handle(
            new CriarPagamentoCommand(ingressoId, 250m, (int)MetodoPagamento.CartaoCredito, "cliente@teste.com"),
            CancellationToken.None);

        resultado.IngressoId.Should().Be(ingressoId);
        resultado.Status.Should().Be("Pendente");
        resultado.Valor.Should().Be(250m);
        await _repositorio.Received(1).AdicionarAsync(Arg.Any<EntidadePagamento>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoIngressoNaoExiste_DeveLancarExcecaoENaoSalvar()
    {
        var ingressoId = Guid.NewGuid();
        _ingressoService.ExisteAsync(ingressoId, Arg.Any<CancellationToken>()).Returns(false);

        var acao = () => _handler.Handle(
            new CriarPagamentoCommand(ingressoId, 100m, (int)MetodoPagamento.Pix, "a@b.com"),
            CancellationToken.None);

        await acao.Should().ThrowAsync<RecursoRelacionadoNaoEncontradoException>()
            .WithMessage($"*{ingressoId}*");
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<EntidadePagamento>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoServicoExternoFalha_DevePropagarExcecao()
    {
        _ingressoService
            .ExisteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("timeout"));

        var acao = () => _handler.Handle(
            new CriarPagamentoCommand(Guid.NewGuid(), 100m, (int)MetodoPagamento.Boleto, "a@b.com"),
            CancellationToken.None);

        await acao.Should().ThrowAsync<HttpRequestException>();
        await _repositorio.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }
}

// ─── AprovarPagamentoCommandHandler ──────────────────────────────────────────

public class AprovarPagamentoCommandHandlerTests
{
    private readonly IPagamentoRepositorio _repositorio = Substitute.For<IPagamentoRepositorio>();
    private readonly IPagamentoEventoPublisher _eventoPublisher = Substitute.For<IPagamentoEventoPublisher>();
    private readonly INotificacaoRealTimeService _notificacaoRealTime = Substitute.For<INotificacaoRealTimeService>();
    private readonly AprovarPagamentoCommandHandler _handler;

    public AprovarPagamentoCommandHandlerTests()
        => _handler = new AprovarPagamentoCommandHandler(_repositorio, _eventoPublisher, _notificacaoRealTime);

    [Fact]
    public async Task Handle_QuandoPagamentoPendente_DeveAprovarSalvarEPublicar()
    {
        var pagamento = new EntidadePagamento(Guid.NewGuid(), 100m, MetodoPagamento.Pix, "cliente@teste.com");
        _repositorio.ObterPorIdAsync(pagamento.Id, Arg.Any<CancellationToken>()).Returns(pagamento);

        var resultado = await _handler.Handle(
            new AprovarPagamentoCommand(pagamento.Id), CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Status.Should().Be("Aprovado");
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await _eventoPublisher.Received(1).PublicarStatusAlteradoAsync(
            pagamento.Id, pagamento.IngressoId, pagamento.Valor,
            "Aprovado", "cliente@teste.com", Arg.Any<CancellationToken>());
        await _notificacaoRealTime.Received(1).NotificarStatusPagamentoAsync(
            "cliente@teste.com", pagamento.Id, pagamento.IngressoId,
            pagamento.Valor, "Aprovado", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoNaoEncontrado_DeveRetornarNullSemSalvar()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var resultado = await _handler.Handle(
            new AprovarPagamentoCommand(Guid.NewGuid()), CancellationToken.None);

        resultado.Should().BeNull();
        await _repositorio.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await _eventoPublisher.DidNotReceiveWithAnyArgs().PublicarStatusAlteradoAsync(
            default, default, default, default!, default!, default);
    }

    [Fact]
    public async Task Handle_QuandoJaAprovado_DevePropagar_InvalidOperationException()
    {
        var pagamento = new EntidadePagamento(Guid.NewGuid(), 100m, MetodoPagamento.CartaoCredito, "a@b.com");
        pagamento.Aprovar();
        _repositorio.ObterPorIdAsync(pagamento.Id, Arg.Any<CancellationToken>()).Returns(pagamento);

        var acao = () => _handler.Handle(new AprovarPagamentoCommand(pagamento.Id), CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*pendentes*");
        await _repositorio.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }
}

// ─── EstornarPagamentoCommandHandler ─────────────────────────────────────────

public class EstornarPagamentoCommandHandlerTests
{
    private readonly IPagamentoRepositorio _repositorio = Substitute.For<IPagamentoRepositorio>();
    private readonly IPagamentoEventoPublisher _eventoPublisher = Substitute.For<IPagamentoEventoPublisher>();
    private readonly INotificacaoRealTimeService _notificacaoRealTime = Substitute.For<INotificacaoRealTimeService>();
    private readonly EstornarPagamentoCommandHandler _handler;

    public EstornarPagamentoCommandHandlerTests()
        => _handler = new EstornarPagamentoCommandHandler(_repositorio, _eventoPublisher, _notificacaoRealTime);

    [Fact]
    public async Task Handle_QuandoAprovado_DeveEstornarEPublicar()
    {
        var pagamento = new EntidadePagamento(Guid.NewGuid(), 350m, MetodoPagamento.Boleto, "comprador@mail.com");
        pagamento.Aprovar();
        _repositorio.ObterPorIdAsync(pagamento.Id, Arg.Any<CancellationToken>()).Returns(pagamento);

        var resultado = await _handler.Handle(
            new EstornarPagamentoCommand(pagamento.Id), CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Status.Should().Be("Estornado");
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await _eventoPublisher.Received(1).PublicarStatusAlteradoAsync(
            pagamento.Id, pagamento.IngressoId, 350m,
            "Estornado", "comprador@mail.com", Arg.Any<CancellationToken>());
        await _notificacaoRealTime.Received(1).NotificarStatusPagamentoAsync(
            "comprador@mail.com", pagamento.Id, pagamento.IngressoId,
            350m, "Estornado", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoNaoEncontrado_DeveRetornarNull()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var resultado = await _handler.Handle(
            new EstornarPagamentoCommand(Guid.NewGuid()), CancellationToken.None);

        resultado.Should().BeNull();
        await _repositorio.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoPendente_DevePropagar_InvalidOperationException()
    {
        var pagamento = new EntidadePagamento(Guid.NewGuid(), 100m, MetodoPagamento.Pix, "a@b.com");
        _repositorio.ObterPorIdAsync(pagamento.Id, Arg.Any<CancellationToken>()).Returns(pagamento);

        var acao = () => _handler.Handle(new EstornarPagamentoCommand(pagamento.Id), CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*aprovados*");
        await _eventoPublisher.DidNotReceiveWithAnyArgs().PublicarStatusAlteradoAsync(
            default, default, default, default!, default!, default);
    }
}
