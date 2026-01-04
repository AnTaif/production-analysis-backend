using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NUnit.Framework;
using ProductionAnalysis.Data.Context;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace ProductionAnalysis.Application.Tests.Infrastructure;

[SetUpFixture]
public class TestContainersFixture
{
    private PostgreSqlContainer postgresContainer = null!;
    private Respawner respawner = null!;
    private DbConnection dbConnection = null!;

    public string ConnectionString { get; private set; } = null!;

    public bool IsInitialized { get; private set; }

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        if (!IsDockerAvailable())
        {
            Console.WriteLine("Docker is not available. Integration tests will be skipped.");
            return;
        }

        postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:18-alpine")
            .WithDatabase("pa_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        await postgresContainer.StartAsync();
        ConnectionString = postgresContainer.GetConnectionString();

        Console.WriteLine($"Test database started: {ConnectionString}");

        await ApplyMigrationsAsync();

        await SetupRespawnAsync();

        IsInitialized = true;
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (IsInitialized)
        {
            await dbConnection.CloseAsync();
            await dbConnection.DisposeAsync();
            await postgresContainer.DisposeAsync();

            Console.WriteLine("Test database stopped");
        }
    }

    public async Task ResetDatabaseAsync()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Test container is not initialized");
        }

        await respawner.ResetAsync(dbConnection);
    }

    private async Task ApplyMigrationsAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("Database schema created");
    }

    private async Task SetupRespawnAsync()
    {
        dbConnection = new NpgsqlConnection(ConnectionString);
        await dbConnection.OpenAsync();

        respawner = await Respawner.CreateAsync(dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Table("__EFMigrationsHistory")],
            WithReseed = true
        });
    }

    private PaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        return new PaDbContext(options);
    }

    private bool IsDockerAvailable()
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}