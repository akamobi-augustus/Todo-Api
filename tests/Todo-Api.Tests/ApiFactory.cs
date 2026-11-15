using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Todo_Api.Tests;

// Starts a real PostgreSQL container once, then starts the API against it.
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:16").Build();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        // Environment variables override appsettings and user-secrets, and they are
        // read before the app starts, so the tests need no secrets on this PC.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _db.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-tests-signing-key-0123456789-abcdef");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "Todo-Api");
        Environment.SetEnvironmentVariable("Jwt__Audience", "Todo-Api-Users");
        Environment.SetEnvironmentVariable("Jwt__ExpiresMinutes", "60");
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }
}

// All test classes share one factory, so only one container is started.
[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<ApiFactory> { }