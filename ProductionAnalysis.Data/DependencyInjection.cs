using Core.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Seeding;

namespace ProductionAnalysis.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfigurationManager config)
    {
        services.AddNpgsqlDbContext<PaDbContext>(config);
        services.AddDataSeeder<DataSeeder>();
        services.AddIdentity();

        services.AddProductionAnalysisData();
        
        return services;
    }

    private static void AddIdentity(this IServiceCollection services)
    {
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
    }
}