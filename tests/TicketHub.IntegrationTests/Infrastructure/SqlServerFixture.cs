using Testcontainers.MsSql;

namespace TicketHub.IntegrationTests.Infrastructure;

/// <summary>
/// Fixture compartilhada que sobe um contêiner SQL Server para todos os testes de integração.
/// Cada banco de dados de serviço é criado como um banco separado no mesmo contêiner.
/// </summary>
public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("TicketHub#Tests2026!")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public string GetConnectionStringPara(string banco) =>
        ConnectionString.Replace("master", banco);

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
