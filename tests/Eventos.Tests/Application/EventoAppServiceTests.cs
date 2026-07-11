using Eventos.Application.Eventos.DTOs;
using Eventos.Application.Eventos.Interfaces;
using Eventos.Application.Eventos.Servicos;
using Eventos.Domain.Entidades;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Eventos.Tests.Application;

public class EventoAppServiceTests
{
    private readonly IEventoRepositorio _repositorio = Substitute.For<IEventoRepositorio>();
    private readonly EventoAppService _sut;

    public EventoAppServiceTests()
    {
        _sut = new EventoAppService(_repositorio);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarESalvar()
    {
        var request = new CriarEventoRequest("Show", "Arena SP", DateTime.UtcNow.AddDays(10), 500);

        var resultado = await _sut.CriarAsync(request, CancellationToken.None);

        resultado.Nome.Should().Be("Show");
        resultado.Status.Should().Be("Planejado");
        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Evento>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoNaoEncontrado_DeveRetornarNulo()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Evento?)null);

        var resultado = await _sut.ObterPorIdAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task PublicarAsync_QuandoEncontrado_DevePublicarESalvar()
    {
        var evento = new Evento("Show", "Arena SP", DateTime.UtcNow.AddDays(10), 500);
        _repositorio.ObterPorIdAsync(evento.Id, Arg.Any<CancellationToken>()).Returns(evento);

        var resultado = await _sut.PublicarAsync(evento.Id, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Status.Should().Be("Publicado");
        await _repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublicarAsync_QuandoNaoEncontrado_DeveRetornarNuloENaoSalvar()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Evento?)null);

        var resultado = await _sut.PublicarAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.Should().BeNull();
        await _repositorio.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EncerrarAsync_QuandoPlanejado_DevePropagarExcecaoDoDominio()
    {
        var evento = new Evento("Show", "Arena SP", DateTime.UtcNow.AddDays(10), 500);
        _repositorio.ObterPorIdAsync(evento.Id, Arg.Any<CancellationToken>()).Returns(evento);

        var acao = async () => await _sut.EncerrarAsync(evento.Id, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ListarAsync_DeveMapearTodosOsResultados()
    {
        var eventos = new List<Evento>
        {
            new("Show 1", "Arena SP", DateTime.UtcNow.AddDays(5), 100),
            new("Show 2", "Arena RJ", DateTime.UtcNow.AddDays(15), 200)
        };
        _repositorio.ListarAsync(Arg.Any<CancellationToken>()).Returns(eventos);

        var resultado = await _sut.ListarAsync(CancellationToken.None);

        resultado.Should().HaveCount(2);
    }
}
