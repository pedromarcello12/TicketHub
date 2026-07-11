using Eventos.Domain.Entidades;
using Eventos.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Eventos.Tests.Domain;

public class EventoTests
{
    private static Evento CriarEventoValido() =>
        new("Show Teste", "Arena SP", DateTime.UtcNow.AddDays(30), 1000);

    [Fact]
    public void Construtor_DeveCriarComStatusPlanejado()
    {
        var evento = CriarEventoValido();

        evento.Status.Should().Be(StatusEvento.Planejado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComNomeInvalido_DeveLancarExcecao(string nome)
    {
        var acao = () => new Evento(nome, "Arena SP", DateTime.UtcNow.AddDays(1), 100);

        acao.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComLocalInvalido_DeveLancarExcecao(string local)
    {
        var acao = () => new Evento("Show", local, DateTime.UtcNow.AddDays(1), 100);

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_ComDataNoPassado_DeveLancarExcecao()
    {
        var acao = () => new Evento("Show", "Arena SP", DateTime.UtcNow.AddDays(-1), 100);

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_ComCapacidadeZeroOuNegativa_DeveLancarExcecao()
    {
        var acao = () => new Evento("Show", "Arena SP", DateTime.UtcNow.AddDays(1), 0);

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Publicar_QuandoPlanejado_DeveMudarParaPublicado()
    {
        var evento = CriarEventoValido();

        evento.Publicar();

        evento.Status.Should().Be(StatusEvento.Publicado);
    }

    [Fact]
    public void Publicar_QuandoJaPublicado_DeveLancarExcecao()
    {
        var evento = CriarEventoValido();
        evento.Publicar();

        var acao = evento.Publicar;

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancelar_QuandoEncerrado_DeveLancarExcecao()
    {
        var evento = CriarEventoValido();
        evento.Publicar();
        evento.Encerrar();

        var acao = evento.Cancelar;

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancelar_QuandoPlanejado_DeveMudarParaCancelado()
    {
        var evento = CriarEventoValido();

        evento.Cancelar();

        evento.Status.Should().Be(StatusEvento.Cancelado);
    }

    [Fact]
    public void Encerrar_QuandoPublicado_DeveMudarParaEncerrado()
    {
        var evento = CriarEventoValido();
        evento.Publicar();

        evento.Encerrar();

        evento.Status.Should().Be(StatusEvento.Encerrado);
    }

    [Fact]
    public void Encerrar_QuandoPlanejado_DeveLancarExcecao()
    {
        var evento = CriarEventoValido();

        var acao = evento.Encerrar;

        acao.Should().Throw<InvalidOperationException>();
    }
}
