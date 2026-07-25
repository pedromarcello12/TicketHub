using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Eventos.Application.Eventos.DTOs;
using Eventos.Infrastructure.Persistencia;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketHub.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketHub.IntegrationTests.Eventos;

[Collection("SqlServer")]
public class EventosIntegrationTests(SqlServerFixture sqlFixture) : IAsyncLifetime
{
    private EventosApiFactory _factory = null!;
    private HttpClient _clientAdmin = null!;
    private HttpClient _clientUsuario = null!;

    public async Task InitializeAsync()
    {
        _factory = new EventosApiFactory(sqlFixture);

        _clientAdmin = _factory.CreateClient();
        _clientAdmin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GerarTokenAdmin());

        _clientUsuario = _factory.CreateClient();
        _clientUsuario.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GerarTokenUsuario());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventosDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _clientAdmin.Dispose();
        _clientUsuario.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CriarEvento_ComoAdmin_DeveRetornar201()
    {
        var request = new CriarEventoRequest(
            "Show de Rock",
            "Arena XYZ",
            DateTime.UtcNow.AddMonths(1),
            1000);

        var response = await _clientAdmin.PostAsJsonAsync("/api/eventos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var evento = await response.Content.ReadFromJsonAsync<EventoResponse>();
        evento!.Nome.Should().Be("Show de Rock");
        evento.Status.Should().Be("Rascunho");
    }

    [Fact]
    public async Task CriarEvento_ComoUsuarioComum_DeveRetornar403()
    {
        var request = new CriarEventoRequest("Show Teste", "Local", DateTime.UtcNow.AddDays(10), 100);

        var response = await _clientUsuario.PostAsJsonAsync("/api/eventos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublicarEvento_DeveAlterarStatusParaPublicado()
    {
        // Cria
        var criarResponse = await _clientAdmin.PostAsJsonAsync("/api/eventos",
            new CriarEventoRequest("Festival Tech", "Centro de Eventos", DateTime.UtcNow.AddMonths(2), 500));
        var evento = await criarResponse.Content.ReadFromJsonAsync<EventoResponse>();

        // Publica
        var publicarResponse = await _clientAdmin.PostAsync($"/api/eventos/{evento!.Id}/publicar", null);

        publicarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var publicado = await publicarResponse.Content.ReadFromJsonAsync<EventoResponse>();
        publicado!.Status.Should().Be("Publicado");
    }

    [Fact]
    public async Task ListarEventos_DeveRetornarListaNaoNula()
    {
        var response = await _clientUsuario.GetAsync("/api/eventos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<EventoResponse>>();
        lista.Should().NotBeNull();
    }
}
