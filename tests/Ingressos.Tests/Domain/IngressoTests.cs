using FluentAssertions;
using Ingressos.Domain.Entidades;
using Ingressos.Domain.Enums;
using Xunit;

namespace Ingressos.Tests.Domain;

public class IngressoTests
{
    private static Ingresso CriarIngressoValido() =>
        new(Guid.NewGuid(), "VIP", 100m);

    [Fact]
    public void Construtor_DeveCriarComStatusDisponivel()
    {
        var ingresso = CriarIngressoValido();

        ingresso.Status.Should().Be(StatusIngresso.Disponivel);
        ingresso.ReservadoAte.Should().BeNull();
    }

    [Fact]
    public void Construtor_ComEventoIdVazio_DeveLancarExcecao()
    {
        var acao = () => new Ingresso(Guid.Empty, "VIP", 100m);

        acao.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComTipoIngressoInvalido_DeveLancarExcecao(string tipoIngresso)
    {
        var acao = () => new Ingresso(Guid.NewGuid(), tipoIngresso, 100m);

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_ComPrecoNegativo_DeveLancarExcecao()
    {
        var acao = () => new Ingresso(Guid.NewGuid(), "VIP", -1m);

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Reservar_QuandoDisponivel_DeveMudarParaReservadoComPrazo()
    {
        var ingresso = CriarIngressoValido();

        ingresso.Reservar();

        ingresso.Status.Should().Be(StatusIngresso.Reservado);
        ingresso.ReservadoAte.Should().BeCloseTo(DateTime.UtcNow.Add(Ingresso.DuracaoReserva), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Reservar_QuandoJaReservado_DeveLancarExcecao()
    {
        var ingresso = CriarIngressoValido();
        ingresso.Reservar();

        var acao = ingresso.Reservar;

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConfirmarVenda_QuandoReservado_DeveMudarParaVendidoELimparPrazo()
    {
        var ingresso = CriarIngressoValido();
        ingresso.Reservar();

        ingresso.ConfirmarVenda();

        ingresso.Status.Should().Be(StatusIngresso.Vendido);
        ingresso.ReservadoAte.Should().BeNull();
    }

    [Fact]
    public void ConfirmarVenda_QuandoDisponivel_DeveLancarExcecao()
    {
        var ingresso = CriarIngressoValido();

        var acao = ingresso.ConfirmarVenda;

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancelar_QuandoVendido_DeveLancarExcecao()
    {
        var ingresso = CriarIngressoValido();
        ingresso.Reservar();
        ingresso.ConfirmarVenda();

        var acao = ingresso.Cancelar;

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancelar_QuandoReservado_DeveMudarParaCanceladoELimparPrazo()
    {
        var ingresso = CriarIngressoValido();
        ingresso.Reservar();

        ingresso.Cancelar();

        ingresso.Status.Should().Be(StatusIngresso.Cancelado);
        ingresso.ReservadoAte.Should().BeNull();
    }

    [Fact]
    public void LiberarReservaExpirada_QuandoAindaNaoExpirou_NaoDeveAlterarStatus()
    {
        var ingresso = CriarIngressoValido();
        ingresso.Reservar();

        ingresso.LiberarReservaExpirada(DateTime.UtcNow);

        ingresso.Status.Should().Be(StatusIngresso.Reservado);
    }

    [Fact]
    public void LiberarReservaExpirada_QuandoExpirou_DeveVoltarParaDisponivel()
    {
        var ingresso = CriarIngressoValido();
        ingresso.Reservar();
        var momentoFuturo = DateTime.UtcNow.Add(Ingresso.DuracaoReserva).AddMinutes(1);

        ingresso.LiberarReservaExpirada(momentoFuturo);

        ingresso.Status.Should().Be(StatusIngresso.Disponivel);
        ingresso.ReservadoAte.Should().BeNull();
    }

    [Fact]
    public void LiberarReservaExpirada_QuandoNaoReservado_NaoDeveAlterarStatus()
    {
        var ingresso = CriarIngressoValido();

        ingresso.LiberarReservaExpirada(DateTime.UtcNow.AddDays(1));

        ingresso.Status.Should().Be(StatusIngresso.Disponivel);
    }
}
