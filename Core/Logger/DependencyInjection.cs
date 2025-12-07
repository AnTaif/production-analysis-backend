using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Core.Logger;

public static class DependencyInjection
{
    public static void UseSerilogLogging(this IHostBuilder hostBuilder, IConfiguration config)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .Enrich.With(new RemovePropertiesEnricher())
            .CreateLogger();

        hostBuilder.UseSerilog();
    }
}