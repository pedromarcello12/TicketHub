using FluentAssertions;
using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Application.Ingressos.Servicos;
using Ingressos.Domain.Entidades;
using Ingressos.Domain.Enums;
using NSubstitute;
using TicketHub.Core.Excecoes;
using Xunit;

namespace Ingressos.Tests.Application;

public class IngressoAppServiceTests
{
    private readonly IIngressoRepositorio _repositorio = Substitute.For<IIngressoRepositorio>();
    private readonly IEventoExternalService _eventoExternalService = Substitute.For<IEventoExternalService>();
    private readonly IDistributedLockService _lockService = Substitute.For<IDistributedLockService>();
    private readonly IngressoAppService _sut;

    public IngressoAppServiceTests()
    {
        _lockService.AdquirirAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IAsyncDisposable>());

        _sut = new IngressoAppService(_repositorio, _eventoExternalService, _lockService);
    }

    [Fact]
    public async Task CriarAsync_QuandoEventoExiste_DeveCriarESalvar()
    {
        var eventoId = Guid.NewGuid();
        _eventoExternalService.ExisteAsync(eventoId, Arg.Any<CancellationToken>()).Returns(true);
        var request = new CriarIngressoRequest(eventoId, "VIP", 150m);

        var resultado = await _sut.CriarAsync(request, CancellationToken.None);

        resultado.EventoId.Should().Be(eventoId);
        resultado.Status.Should().Be("Disponivel");
        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Ingresso>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_QuandoEventoNaoExiste_DeveLancarExcecaoENaoSalvar()
    {
        var eventoId = Guid.NewGuid();
        _eventoExternalService.ExisteAsync(eventoId, Arg.Any<CancellationToken>()).Returns(false);
        var request = new CriarIngressoRequest(eventoId, "VIP", 150m);

        var acao = async () => await _sut.CriarAsync(request, CancellationToken.None);

        await acao.Should().ThrowAsync<RecursoRelacionadoNaoEncontradoException>();
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Ingresso>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoNaoEncontrado_DeveRetornarNulo()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Ingresso?)null);

        var resultado = await _sut.ObterPorIdAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ReservarAsync_QuandoEncontrado_DeveReservarESalvar()
    {
        var ingresso = new Ingresso(Guid.NewGuid(), "VIP", 100m);
        _repositorio.ObterPorIdAsync(ingresso.Id, Arg.Any<CancellationToken>()).Returns(ingresso);

        var resultado = await _sut.ReservarAsync(ingresso.Id, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Status.Should().Be("Reservado");
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReservarAsync_QuandoNaoEncontrado_DeveRetornarNuloENaoSalvar()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Ingresso?)null);

        var resultado = await _sut.ReservarAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.Should().BeNull();
        await _repositorio.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReservarAsync_QuandoNaoConsegueAdquirirLock_DeveLancarExcecaoENaoConsultarRepositorio()
    {
        _lockService.AdquirirAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((IAsyncDisposable?)null);

        var acao = async () => await _sut.ReservarAsync(Guid.NewGuid(), CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
        await _repositorio.DidNotReceive().ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LiberarReservasExpiradasAsync_SemExpirados_NaoDeveSalvar()
    {
        _repositorio.ListarReservasExpiradasAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Ingresso>());

        var quantidade = await _sut.LiberarReservasExpiradasAsync(CancellationToken.None);

        quantidade.Should().Be(0);
        await _repositorio.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LiberarReservasExpiradasAsync_ComExpirados_DeveLiberarTodosESalvar()
    {
        var ingresso1 = new Ingresso(Guid.NewGuid(), "VIP", 100m);
        ingresso1.Reservar();
        ForcarReservaExpirada(ingresso1);
        var ingresso2 = new Ingresso(Guid.NewGuid(), "Pista", 50m);
        ingresso2.Reservar();
        ForcarReservaExpirada(ingresso2);

        _repositorio.ListarReservasExpiradasAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Ingresso> { ingresso1, ingresso2 });

        var quantidade = await _sut.LiberarReservasExpiradasAsync(CancellationToken.None);

        quantidade.Should().Be(2);
        ingresso1.Status.Should().Be(StatusIngresso.Disponivel);
        ingresso2.Status.Should().Be(StatusIngresso.Disponivel);
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    private static void ForcarReservaExpirada(Ingresso ingresso)
    {
        typeof(Ingresso)
            .GetProperty(nameof(Ingresso.ReservadoAte))!
            .SetValue(ingresso, DateTime.UtcNow.AddMinutes(-1));
    }
}
