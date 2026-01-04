namespace ProductionAnalysis.Application.Domain.Forms;

/// <summary>
/// Контекст одного продукта
/// </summary>
public class ProductContext : FormContext
{
    public ProductContext(
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

    public int ProductId { get; }
    public int? CycleTime { get; }
    public int? WorkstationCapacity { get; }
    public int DailyRate { get; }
}