using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Ingressos.Application.Ingressos.DTOs;
using Ingressos.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TicketHub.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketHub.IntegrationTests.Ingressos;

[Collection("SqlServer")]
public class IngressosIntegrationTests(SqlServerFixture sqlFixture) : IAsyncLifetime
{
    private IngressosApiFactory _factory = null!;
    private HttpClient _clientAdmin = null!;
    private HttpClient _clientUsuario = null!;

    public async Task InitializeAsync()
    {
        _factory = new IngressosApiFactory(sqlFixture);

        _clientAdmin = _factory.CreateClient();
        _clientAdmin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GerarTokenAdmin());

        _clientUsuario = _factory.CreateClient();
        _clientUsuario.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GerarTokenUsuario());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IngressosDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _clientAdmin.Dispose();
        _clientUsuario.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CriarIngresso_QuandoEventoExiste_DeveRetornar201()
    {
        var eventoId = Guid.NewGuid();
        _factory.EventoExternalServiceMock
            .ExisteAsync(eventoId, Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new CriarIngressoRequest(eventoId, "VIP", 250m);

        var response = await _clientAdmin.PostAsJsonAsync("/api/ingressos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ingresso = await response.Content.ReadFromJsonAsync<IngressoResponse>();
        ingresso!.Status.Should().Be("Disponivel");
        ingresso.Preco.Should().Be(250m);
    }

    [Fact]
    public async Task CriarIngresso_QuandoEventoNaoExiste_DeveRetornar422()
    {
        var eventoId = Guid.NewGuid();
        _factory.EventoExternalServiceMock
            .ExisteAsync(eventoId, Arg.Any<CancellationToken>())
            .Returns(false);

        var request = new CriarIngressoRequest(eventoId, "Pista", 100m);

        var response = await _clientAdmin.PostAsJsonAsync("/api/ingressos", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ReservarIngresso_DeveAlterarStatusParaReservado()
    {
        var eventoId = Guid.NewGuid();
        _factory.EventoExternalServiceMock
            .ExisteAsync(eventoId, Arg.Any<CancellationToken>())
            .Returns(true);

        var criarResponse = await _clientAdmin.PostAsJsonAsync("/api/ingressos",
            new CriarIngressoRequest(eventoId, "Pista", 80m));
        var ingresso = await criarResponse.Content.ReadFromJsonAsync<IngressoResponse>();

        var reservarResponse = await _clientUsuario.PostAsync($"/api/ingressos/{ingresso!.Id}/reservar", null);

        reservarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reservado = await reservarResponse.Content.ReadFromJsonAsync<IngressoResponse>();
        reservado!.Status.Should().Be("Reservado");
        reservado.ReservadoAte.Should().NotBeNull();
    }

    [Fact]
    public async Task FluxoCompleto_CriarReservarConfirmarVenda()
    {
        var eventoId = Guid.NewGuid();
        _factory.EventoExternalServiceMock
            .ExisteAsync(eventoId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Criar
        var criarResp = await _clientAdmin.PostAsJsonAsync("/api/ingressos",
            new CriarIngressoRequest(eventoId, "Camarote", 500m));
        var ingresso = await criarResp.Content.ReadFromJsonAsync<IngressoResponse>();

        // Reservar
        await _clientUsuario.PostAsync($"/api/ingressos/{ingresso!.Id}/reservar", null);

        // Confirmar venda (admin confirma após pagamento)
        var confirmarResp = await _clientAdmin.PostAsync($"/api/ingressos/{ingresso.Id}/confirmar-venda", null);

        confirmarResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var vendido = await confirmarResp.Content.ReadFromJsonAsync<IngressoResponse>();
        vendido!.Status.Should().Be("Vendido");
        vendido.ReservadoAte.Should().BeNull();
    }
}
