using Core.Database;
using DotNetEnv;
using Microsoft.OpenApi;
using ProductionAnalysis.Api;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

ServiceConfiguration.ConfigureServices(builder.Services, builder.Configuration, builder.Host);

var app = builder.Build();

await app.TrySeedDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => { c.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0; });
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();