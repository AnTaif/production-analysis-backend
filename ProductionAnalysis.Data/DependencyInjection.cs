using Core.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductionAnalysis.Data.Context;

namespace ProductionAnalysis.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfigurationManager config)
    {
        services.AddNpgsqlDbContext<PaDbContext>(config);

        services.AddProductionAnalysisData();
        
        return services;
    }
}