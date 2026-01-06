using Microsoft.Extensions.DependencyInjection;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

namespace ProductionAnalysis.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddProductionAnalysisApplication();

        services.AddScoped<IRowInitializationStrategy, LessThanOnePerHourInitializationStrategy>();
        services.AddScoped<IRowInitializationStrategy, LessThanOnePerShiftInitializationStrategy>();
        services.AddScoped<IRowInitializationStrategy, MultipleProductsWithCycleTimeInitializationStrategy>();
        services.AddScoped<IRowInitializationStrategy, SingleProductWithCycleTimeInitializationStrategy>();
        services.AddScoped<IRowInitializationStrategy, SingleProductWithWorkstationCapacityInitializationStrategy>();

        return services;
    }
}