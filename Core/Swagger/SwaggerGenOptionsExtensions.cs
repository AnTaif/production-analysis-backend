using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Core.Swagger;

public static class SwaggerGenOptionsExtensions
{
    public static void AddDocs(this SwaggerGenOptions options)
    {
        options.ExampleFilters();

        // Включаем XML-комментарии из API проекта
        var apiXmlFile = $"{Assembly.GetCallingAssembly().GetName().Name}.xml";
        var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
        if (File.Exists(apiXmlPath))
        {
            options.IncludeXmlComments(apiXmlPath);
        }

        // Включаем XML-комментарии из Client.Models проекта
        try
        {
            var clientModelsAssembly = Assembly.Load("ProductionAnalysis.Client.Models");
            var clientModelsXmlFile = $"{clientModelsAssembly.GetName().Name}.xml";

            // Пробуем найти XML-файл в разных местах
            var possiblePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, clientModelsXmlFile),
                Path.Combine(Path.GetDirectoryName(clientModelsAssembly.Location) ?? "", clientModelsXmlFile)
            };

            foreach (var xmlPath in possiblePaths)
            {
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                    break;
                }
            }
        }
        catch
        {
            // Игнорируем ошибки, если сборка не найдена
        }
    }

    public static void AddJwtSecurity(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}