using FluentAssertions;
using Pagamento.Domain.Enums;
using Xunit;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Tests.Domain;

/// <summary>
/// Testes unitários para a state machine da entidade Pagamento.
/// Cobre todas as transições válidas e todas as transições proibidas.
/// </summary>
public class PagamentoTests
{
    // ─── Fábrica ──────────────────────────────────────────────────────────────

    private static EntidadePagamento CriarPagamentoValido(
        decimal valor = 100m,
        string email = "cliente@teste.com",
        MetodoPagamento metodo = MetodoPagamento.Pix)
        => new(Guid.NewGuid(), valor, metodo, email);

    // ─── Construtor ───────────────────────────────────────────────────────────

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

        acao.Should().Throw<ArgumentException>()
            .WithMessage("*ingresso*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Construtor_ComValorInvalido_DeveLancarExcecao(decimal valor)
    {
        var acao = () => new EntidadePagamento(Guid.NewGuid(), valor, MetodoPagamento.Pix, "cliente@teste.com");

        acao.Should().Throw<ArgumentException>()
            .WithMessage("*valor*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba")]
    public void Construtor_ComEmailInvalido_DeveLancarExcecao(string email)
    {
        var acao = () => new EntidadePagamento(Guid.NewGuid(), 100m, MetodoPagamento.Pix, email);

        acao.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Fact]
    public void Construtor_DevePersistirDadosCorretamente()
    {
        var ingressoId = Guid.NewGuid();

        var pagamento = new EntidadePagamento(ingressoId, 350m, MetodoPagamento.CartaoCredito, "comprador@mail.com");

        pagamento.IngressoId.Should().Be(ingressoId);
        pagamento.Valor.Should().Be(350m);
        pagamento.Metodo.Should().Be(MetodoPagamento.CartaoCredito);
        pagamento.EmailCliente.Should().Be("comprador@mail.com");
        pagamento.Id.Should().NotBeEmpty();
    }

    // ─── Aprovar ──────────────────────────────────────────────────────────────

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

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*pendentes*");
    }

    [Fact]
    public void Aprovar_QuandoRecusado_DeveLancarExcecao()
    {
        var pagamento = CriarPagamentoValido();
        pagamento.Recusar();

        var acao = pagamento.Aprovar;

        acao.Should().Throw<InvalidOperationException>();
    }

    // ─── Recusar ──────────────────────────────────────────────────────────────

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

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*pendentes*");
    }

    // ─── Estornar ─────────────────────────────────────────────────────────────

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

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*aprovados*");
    }

    [Fact]
    public void Estornar_QuandoRecusado_DeveLancarExcecao()
    {
        var pagamento = CriarPagamentoValido();
        pagamento.Recusar();

        var acao = pagamento.Estornar;

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Estornar_QuandoJaEstornado_DeveLancarExcecao()
    {
        var pagamento = CriarPagamentoValido();
        pagamento.Aprovar();
        pagamento.Estornar();

        var acao = pagamento.Estornar;

        acao.Should().Throw<InvalidOperationException>();
    }

    // ─── Fluxos completos ─────────────────────────────────────────────────────

    [Fact]
    public void FluxoCompleto_PendenteAprovadoEstornado_DevePassarSemExcecao()
    {
        var pagamento = CriarPagamentoValido();

        pagamento.Status.Should().Be(StatusPagamento.Pendente);
        pagamento.Aprovar();
        pagamento.Status.Should().Be(StatusPagamento.Aprovado);
        pagamento.Estornar();
        pagamento.Status.Should().Be(StatusPagamento.Estornado);
    }

    [Fact]
    public void FluxoCompleto_PendenteRecusado_DevePassarSemExcecao()
    {
        var pagamento = CriarPagamentoValido();

        pagamento.Recusar();

        pagamento.Status.Should().Be(StatusPagamento.Recusado);
    }
}
