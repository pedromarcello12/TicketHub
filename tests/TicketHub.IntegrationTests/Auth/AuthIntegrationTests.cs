using System.Net;
using System.Net.Http.Json;
using Auth.Application.Auth.DTOs;
using Auth.Infrastructure.Persistencia;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketHub.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketHub.IntegrationTests.Auth;

[Collection("SqlServer")]
public class AuthIntegrationTests(SqlServerFixture sqlFixture) : IAsyncLifetime
{
    private AuthApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new AuthApiFactory(sqlFixture);
        _client = _factory.CreateClient();

        // Garante que o banco existe e aplica migrations
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Registrar_DeveRetornar201ComToken()
    {
        var request = new RegistrarUsuarioRequest("João Teste", $"joao_{Guid.NewGuid():N}@test.com", "Senha@123");

        var response = await _client.PostAsJsonAsync("/api/auth/registrar", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var corpo = await response.Content.ReadAsStringAsync();
        corpo.Should().Contain("token");
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornar200ComToken()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/registrar",
            new RegistrarUsuarioRequest("Login Teste", email, "Senha@123"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "Senha@123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var corpo = await response.Content.ReadAsStringAsync();
        corpo.Should().Contain("token");
        corpo.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Login_ComSenhaErrada_DeveRetornar401()
    {
        var email = $"invalido_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/registrar",
            new RegistrarUsuarioRequest("Invalido", email, "Senha@123"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "SenhaErrada@999"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
