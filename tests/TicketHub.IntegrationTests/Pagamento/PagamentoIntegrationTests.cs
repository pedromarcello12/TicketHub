using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pagamento.Application.Pagamentos.DTOs;
using Pagamento.Infrastructure.Persistencia;
using TicketHub.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketHub.IntegrationTests.Pagamento;

[Collection("SqlServer")]
public class PagamentoIntegrationTests(SqlServerFixture sqlFixture) : IAsyncLifetime
{
    private PagamentoApiFactory _factory = null!;
    private HttpClient _clientAdmin = null!;
    private HttpClient _clientUsuario = null!;

    public async Task InitializeAsync()
    {
        _factory = new PagamentoApiFactory(sqlFixture);

        _clientAdmin = _factory.CreateClient();
        _clientAdmin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GerarTokenAdmin());

        _clientUsuario = _factory.CreateClient();
        _clientUsuario.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GerarTokenUsuario());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PagamentosDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _clientAdmin.Dispose();
        _clientUsuario.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CriarPagamento_QuandoIngressoExiste_DeveRetornar201()
    {
        var ingressoId = Guid.NewGuid();
        _factory.IngressoExternalServiceMock
            .ExisteAsync(ingressoId, Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new CriarPagamentoRequest(ingressoId, 250m, "CartaoCredito", "usuario@test.com");

        var response = await _clientUsuario.PostAsJsonAsync("/api/pagamentos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var pagamento = await response.Content.ReadFromJsonAsync<PagamentoResponse>();
        pagamento!.Status.Should().Be("Pendente");
        pagamento.Valor.Should().Be(250m);
    }

    [Fact]
    public async Task AprovarPagamento_DeveAlterarStatusEPublicarEvento()
    {
        var ingressoId = Guid.NewGuid();
        _factory.IngressoExternalServiceMock
            .ExisteAsync(ingressoId, Arg.Any<CancellationToken>())
            .Returns(true);
        _factory.EventoPublisherMock
            .PublicarStatusAlteradoAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<decimal>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var criarResp = await _clientUsuario.PostAsJsonAsync("/api/pagamentos",
            new CriarPagamentoRequest(ingressoId, 100m, "Pix", "usuario@test.com"));
        var pagamento = await criarResp.Content.ReadFromJsonAsync<PagamentoResponse>();

        var aprovarResp = await _clientAdmin.PostAsync($"/api/pagamentos/{pagamento!.Id}/aprovar", null);

        aprovarResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var aprovado = await aprovarResp.Content.ReadFromJsonAsync<PagamentoResponse>();
        aprovado!.Status.Should().Be("Aprovado");

        // Verifica que o evento foi publicado
        await _factory.EventoPublisherMock.Received(1)
            .PublicarStatusAlteradoAsync(
                pagamento.Id, ingressoId, 100m, "Aprovado", "usuario@test.com",
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FluxoCompleto_CriarAprovarEstornar()
    {
        var ingressoId = Guid.NewGuid();
        _factory.IngressoExternalServiceMock
            .ExisteAsync(ingressoId, Arg.Any<CancellationToken>())
            .Returns(true);
        _factory.EventoPublisherMock
            .PublicarStatusAlteradoAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<decimal>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var criarResp = await _clientUsuario.PostAsJsonAsync("/api/pagamentos",
            new CriarPagamentoRequest(ingressoId, 200m, "CartaoDebito", "cliente@test.com"));
        var pagamento = await criarResp.Content.ReadFromJsonAsync<PagamentoResponse>();

        await _clientAdmin.PostAsync($"/api/pagamentos/{pagamento!.Id}/aprovar", null);

        var estornarResp = await _clientAdmin.PostAsync($"/api/pagamentos/{pagamento.Id}/estornar", null);

        estornarResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var estornado = await estornarResp.Content.ReadFromJsonAsync<PagamentoResponse>();
        estornado!.Status.Should().Be("Estornado");
    }
}
