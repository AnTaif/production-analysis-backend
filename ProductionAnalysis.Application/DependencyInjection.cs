using Microsoft.Extensions.DependencyInjection;

namespace ProductionAnalysis.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddProductionAnalysisApplication();
        
        return services;
    }
}