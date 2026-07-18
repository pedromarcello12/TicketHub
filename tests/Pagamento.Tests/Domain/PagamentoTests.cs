using FluentAssertions;
using Pagamento.Domain.Enums;
using Xunit;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Tests.Domain;

public class PagamentoTests
{
    private static EntidadePagamento CriarPagamentoValido() =>
        new(Guid.NewGuid(), 100m, MetodoPagamento.Pix, "cliente@teste.com");

    [Fact]
    public void Construtor_DeveCriarComStatusPendente()
    {
        var pagamento = CriarPagamentoValido();

        pagamento.Status.Should().Be(StatusPagamento.Pendente);
    }

    [Fact]
    public void Construtor_ComIngressoIdVazio_DeveLancarExcecao()
    {
        var acao = () => new EntidadePagamento(Guid.Empty, 100m, MetodoPagamento.Pix, "cliente@teste.com");

        acao.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Construtor_ComValorInvalido_DeveLancarExcecao(decimal valor)
    {
        var acao = () => new EntidadePagamento(Guid.NewGuid(), valor, MetodoPagamento.Pix, "cliente@teste.com");

        acao.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba")]
    public void Construtor_ComEmailInvalido_DeveLancarExcecao(string email)
    {
        var acao = () => new EntidadePagamento(Guid.NewGuid(), 100m, MetodoPagamento.Pix, email);

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Aprovar_QuandoPendente_DeveMudarParaAprovado()
    {
        var pagamento = CriarPagamentoValido();

        pagamento.Aprovar();

        pagamento.Status.Should().Be(StatusPagamento.Aprovado);
    }

    [Fact]
    public void Aprovar_QuandoJaAprovado_DeveLancarExcecao()
    {
        var pagamento = CriarPagamentoValido();
        pagamento.Aprovar();

        var acao = pagamento.Aprovar;

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Recusar_QuandoPendente_DeveMudarParaRecusado()
    {
        var pagamento = CriarPagamentoValido();

        pagamento.Recusar();

        pagamento.Status.Should().Be(StatusPagamento.Recusado);
    }

    [Fact]
    public void Recusar_QuandoAprovado_DeveLancarExcecao()
    {
        var pagamento = CriarPagamentoValido();
        pagamento.Aprovar();

        var acao = pagamento.Recusar;

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Estornar_QuandoAprovado_DeveMudarParaEstornado()
    {
        var pagamento = CriarPagamentoValido();
        pagamento.Aprovar();

        pagamento.Estornar();

        pagamento.Status.Should().Be(StatusPagamento.Estornado);
    }

    [Fact]
    public void Estornar_QuandoPendente_DeveLancarExcecao()
    {
        var pagamento = CriarPagamentoValido();

        var acao = pagamento.Estornar;

        acao.Should().Throw<InvalidOperationException>();
    }
}
