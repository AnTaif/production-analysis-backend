using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Repositories;

namespace ProductionAnalysis.Application.Tests.Infrastructure;

[TestFixture]
public abstract class BaseIntegrationTest
{
    private static readonly TestContainersFixture TestContainersFixture = new();
    private IServiceProvider? serviceProvider;
    private IServiceScope? scope;

    protected IPaUnitOfWork UnitOfWork { get; private set; } = null!;
    protected IFormsService FormsService { get; private set; } = null!;
    protected PaDbContext DbContext { get; private set; } = null!;
    protected TestDataBuilder DataBuilder { get; private set; } = null!;

    [OneTimeSetUp]
    public static async Task OneTimeSetUp()
    {
        if (!IsDockerAvailable())
        {
            Assert.Ignore("Docker is not available. Skipping integration tests.");
            return;
        }

        await TestContainersFixture.GlobalSetup();
    }

    [SetUp]
    public virtual async Task SetUp()
    {
        if (!TestContainersFixture.IsInitialized)
        {
            Assert.Ignore("Test container is not initialized. Docker may not be available.");
            return;
        }

        await TestContainersFixture.ResetDatabaseAsync();

        var services = new ServiceCollection();

        // Регистрация логирования
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // Настройка базы данных
        services.AddDbContext<PaDbContext>(options =>
        {
            options.UseNpgsql(TestContainersFixture.ConnectionString);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        // Настройка Identity
        services.AddIdentity<UserDbo, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<PaDbContext>()
            .AddDefaultTokenProviders();

        // Регистрация репозиториев
        services.AddScoped<IDictionariesRepository, DictionariesRepository>();
        services.AddScoped<IFormsRepository, FormsRepository>();
        services.AddScoped<IFormRowsRepository, FormRowsRepository>();
        services.AddScoped<ITemplatesRepository, TemplatesRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<IPaUnitOfWork, PaUnitOfWork>();

        // Регистрация сервисов приложения
        services.AddScoped<IPlanCalculator, PlanCalculator>();
        services.AddScoped<IFormRowInitializer, FormRowInitializer>();
        services.AddScoped<IFormRowValueFilter, FormRowValueFilter>();
        services.AddScoped<IFormRowFormulaCalculator, FormRowFormulaCalculator>();
        services.AddScoped<ICumulativeValueCalculator, CumulativeValueCalculator>();
        services.AddScoped<IFormulaCalculator, FormulaCalculator>();
        services.AddScoped<IProductContextExtractor, ProductContextExtractor>();
        services.AddScoped<IFormRowDataFactory, FormRowDataFactory>();
        services.AddScoped<IFormsService, FormsService>();

        serviceProvider = services.BuildServiceProvider();
        scope = serviceProvider.CreateScope();

        UnitOfWork = scope.ServiceProvider.GetRequiredService<IPaUnitOfWork>();
        FormsService = scope.ServiceProvider.GetRequiredService<IFormsService>();
        DbContext = scope.ServiceProvider.GetRequiredService<PaDbContext>();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDbo>>();
        DataBuilder = new TestDataBuilder(DbContext, userManager);
    }

    [TearDown]
    public virtual Task TearDown()
    {
        scope?.Dispose();
        return Task.CompletedTask;
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        if (TestContainersFixture.IsInitialized)
        {
            await TestContainersFixture.GlobalTeardown();
        }
    }

    private static bool IsDockerAvailable()
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