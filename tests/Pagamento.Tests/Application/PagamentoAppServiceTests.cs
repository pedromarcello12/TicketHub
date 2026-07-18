using FluentAssertions;
using NSubstitute;
using Pagamento.Application.Pagamentos.DTOs;
using Pagamento.Application.Pagamentos.Interfaces;
using Pagamento.Application.Pagamentos.Servicos;
using Pagamento.Domain.Enums;
using TicketHub.Core.Excecoes;
using Xunit;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Tests.Application;

public class PagamentoAppServiceTests
{
    private readonly IPagamentoRepositorio _repositorio = Substitute.For<IPagamentoRepositorio>();
    private readonly IPagamentoEventoPublisher _eventoPublisher = Substitute.For<IPagamentoEventoPublisher>();
    private readonly IIngressoExternalService _ingressoExternalService = Substitute.For<IIngressoExternalService>();
    private readonly PagamentoAppService _sut;

    public PagamentoAppServiceTests()
    {
        _sut = new PagamentoAppService(_repositorio, _eventoPublisher, _ingressoExternalService);
    }

    [Fact]
    public async Task CriarAsync_QuandoIngressoExiste_DeveCriarESalvar()
    {
        var ingressoId = Guid.NewGuid();
        _ingressoExternalService.ExisteAsync(ingressoId, Arg.Any<CancellationToken>()).Returns(true);
        var request = new CriarPagamentoRequest(ingressoId, 250m, MetodoPagamento.CartaoCredito, "cliente@teste.com");

        var resultado = await _sut.CriarAsync(request, CancellationToken.None);

        resultado.IngressoId.Should().Be(ingressoId);
        resultado.Status.Should().Be("Pendente");
        await _repositorio.Received(1).AdicionarAsync(Arg.Any<EntidadePagamento>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_QuandoIngressoNaoExiste_DeveLancarExcecaoENaoSalvar()
    {
        var ingressoId = Guid.NewGuid();
        _ingressoExternalService.ExisteAsync(ingressoId, Arg.Any<CancellationToken>()).Returns(false);
        var request = new CriarPagamentoRequest(ingressoId, 250m, MetodoPagamento.Pix, "cliente@teste.com");

        var acao = async () => await _sut.CriarAsync(request, CancellationToken.None);

        await acao.Should().ThrowAsync<RecursoRelacionadoNaoEncontradoException>();
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<EntidadePagamento>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AprovarAsync_QuandoEncontrado_DeveAprovarSalvarEPublicarEvento()
    {
        var pagamento = new EntidadePagamento(Guid.NewGuid(), 100m, MetodoPagamento.Pix, "cliente@teste.com");
        _repositorio.ObterPorIdAsync(pagamento.Id, Arg.Any<CancellationToken>()).Returns(pagamento);

        var resultado = await _sut.AprovarAsync(pagamento.Id, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Status.Should().Be("Aprovado");
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await _eventoPublisher.Received(1).PublicarStatusAlteradoAsync(
            pagamento.Id, pagamento.IngressoId, pagamento.Valor, "Aprovado", pagamento.EmailCliente, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AprovarAsync_QuandoNaoEncontrado_DeveRetornarNuloENaoPublicarEvento()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EntidadePagamento?)null);

        var resultado = await _sut.AprovarAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.Should().BeNull();
        await _eventoPublisher.DidNotReceive().PublicarStatusAlteradoAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EstornarAsync_QuandoPendente_DevePropagarExcecaoDoDominioENaoPublicarEvento()
    {
        var pagamento = new EntidadePagamento(Guid.NewGuid(), 100m, MetodoPagamento.Pix, "cliente@teste.com");
        _repositorio.ObterPorIdAsync(pagamento.Id, Arg.Any<CancellationToken>()).Returns(pagamento);

        var acao = async () => await _sut.EstornarAsync(pagamento.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
        await _eventoPublisher.DidNotReceive().PublicarStatusAlteradoAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListarAsync_DeveMapearTodosOsResultados()
    {
        var pagamentos = new List<EntidadePagamento>
        {
            new(Guid.NewGuid(), 100m, MetodoPagamento.Pix, "cliente1@teste.com"),
            new(Guid.NewGuid(), 200m, MetodoPagamento.Boleto, "cliente2@teste.com")
        };
        _repositorio.ListarAsync(null, Arg.Any<CancellationToken>()).Returns(pagamentos);

        var resultado = await _sut.ListarAsync(null, CancellationToken.None);

        resultado.Should().HaveCount(2);
    }
}
