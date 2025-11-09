using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Database;

public static class DependencyInjection
{
    public static void AddNpgsqlDbContext<TContext>(this IServiceCollection services, IConfigurationManager config)
        where TContext : DbContext
    {
        var dbOptions = new DatabaseOptions();
        config.GetSection("DatabaseOptions").Bind(dbOptions);
        dbOptions.Host = Environment.GetEnvironmentVariable("DB_CONTAINER") ?? "localhost";
        dbOptions.Port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "3306";
        dbOptions.User = Environment.GetEnvironmentVariable("DATABASE_USER")!;
        dbOptions.Password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD")!;

        services.Configure<DatabaseOptions>(options =>
        {
            options.Name = dbOptions.Name;
            options.Host = dbOptions.Host;
            options.Port = dbOptions.Port;
            options.User = dbOptions.User;
            options.Password = dbOptions.Password;
        });

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(dbOptions.GetConnectionString());
        });
    }

    public static void AddDataSeeder<TDataSeeder>(this IServiceCollection services)
        where TDataSeeder : class, IDataSeeder
    {
        services.AddTransient<IDataSeeder, TDataSeeder>();
    }

    public static async Task<bool> TrySeedDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        return await dataSeeder.TrySeedAsync();
    }
}