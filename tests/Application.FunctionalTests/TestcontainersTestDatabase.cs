using System.Data.Common;
using N1coLoyalty.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Testcontainers.MsSql;

namespace N1coLoyalty.Application.FunctionalTests;

public class TestContainersTestDatabase : ITestDatabase
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithAutoRemove(true)
        .Build();
    private DbConnection _connection = null!;
    private string _connectionString = null!;
    private Respawner _respawner = null!;

    public async Task InitialiseAsync()
    {
        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        _connection = new SqlConnection(_connectionString);

        // Prepare ApplicationDbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        var context = new ApplicationDbContext(options);

        await context.Database.MigrateAsync();
        
        // Prepare Respawn
        _respawner = await Respawner.CreateAsync(_connectionString, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory", "RewardType"]
        });
    }

    public DbConnection GetConnection()
    {
        return _connection;
    }

    public async Task ResetAsync()
    {
        await _respawner.ResetAsync(_connectionString);
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
