using TicketHub.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketHub.IntegrationTests;

/// <summary>
/// Garante que o contêiner SQL Server é compartilhado entre todos os testes
/// na collection, evitando subir múltiplos contêineres em paralelo.
/// </summary>
[CollectionDefinition("SqlServer")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>;
