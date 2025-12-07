using System.Reflection;
using Core.Auth;
using Core.Logger;
using Core.Swagger;
using FluentValidation;
using Microsoft.OpenApi.Models;
using ProductionAnalysis.Application;
using ProductionAnalysis.Data;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api;

public static class ServiceConfiguration
{
    public static void ConfigureServices(
        IServiceCollection services,
        IConfiguration configuration,
        IHostBuilder host)
    {
        services.AddControllers();
        services.AddValidation();
        services.AddSwagger();
        services.AddDataLayer(configuration);
        services.AddApplicationLayer();
        services.AddJwtAuth(configuration);
        services.AddHealthChecks();
        services.AddCors(configuration);

        host.UseSerilogLogging(configuration);
    }

    private static void AddValidation(this IServiceCollection services)
    {
        services
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddFluentValidationAutoValidation();
    }

    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ProductionAnalysis API",
                    Version = "v1"
                });
                options.AddJwtSecurity();
                options.AddDocs();
            })
            .AddSwaggerExamplesFromAssemblies(Assembly.GetExecutingAssembly());
    }

    private static void AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins = configuration.GetSection("AllowedOrigins").Get<string[]?>();
        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                if (corsOrigins != null)
                    policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
                else
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });
    }
}