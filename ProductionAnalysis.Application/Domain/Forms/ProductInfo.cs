namespace ProductionAnalysis.Application.Domain.Forms;

/// <summary>
/// Информация о продукте в контексте формы
/// </summary>
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

    public int ProductId { get; }
    public int? CycleTime { get; }
    public int? WorkstationCapacity { get; }
    public int DailyRate { get; }
}