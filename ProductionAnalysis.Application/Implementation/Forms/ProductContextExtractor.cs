using System.Text.Json;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IProductContextExtractor
{
    ProductContext? Extract(Dictionary<string, object>? formContext);
}

[RegisterScoped]
public class ProductContextExtractor : IProductContextExtractor
{
    public ProductContext? Extract(Dictionary<string, object>? formContext)
    {
        if (formContext == null)
        {
            return null;
        }

        foreach (var (_, value) in formContext)
        {
            if (value is not JsonElement jsonElement)
            {
                continue;
            }

            if (!jsonElement.TryGetProperty("dailyRate", out var dailyRateElement))
            {
                continue;
            }

            var dailyRate = dailyRateElement.GetInt32();
            jsonElement.TryGetProperty("cycleTime", out var cycleTimeElement);

            return new ProductContext
            {
                DailyRate = dailyRate,
                CycleTime = cycleTimeElement.GetInt32()
            };
        }

        return null;
    }
}