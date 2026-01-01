using System.Text.Json.Serialization;

namespace ProductionAnalysis.Application.Domain.Forms;

public class ProductFormContext : FormContextBase
{
    [JsonPropertyName("productId")]
    public int? ProductId { get; set; }

    [JsonPropertyName("cycleTime")]
    public int? CycleTime { get; set; }

    [JsonPropertyName("workstationCapacity")]
    public int? WorkstationCapacity { get; set; }

    [JsonPropertyName("dailyRate")]
    public required int DailyRate { get; set; }
}