using System.Text.Json.Serialization;

namespace ProductionAnalysis.Application.Domain.Forms;

public class MultiProductFormContext : FormContextBase
{
    public MultiProductFormContext(ICollection<ProductInfo> products)
    {
        Products = products;
    }

    [JsonPropertyName("products")]
    public ICollection<ProductInfo> Products { get; }
}

public class ProductInfo
{
    public ProductInfo(
        int productId,
        int? cycleTime,
        int? workstationCapacity,
        int dailyRate)
    {
        ProductId = productId;
        CycleTime = cycleTime;
        WorkstationCapacity = workstationCapacity;
        DailyRate = dailyRate;
    }

    [JsonPropertyName("productId")]
    public int ProductId { get; }

    [JsonPropertyName("cycleTime")]
    public int? CycleTime { get; }

    [JsonPropertyName("workstationCapacity")]
    public int? WorkstationCapacity { get; }

    [JsonPropertyName("dailyRate")]
    public int DailyRate { get; }
}