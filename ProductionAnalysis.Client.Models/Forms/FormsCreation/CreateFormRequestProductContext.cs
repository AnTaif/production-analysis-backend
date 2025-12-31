using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms.FormsCreation;

public class CreateFormRequestProductContext : CreateFormRequestContextBase
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int? CycleTime { get; init; }

    [Range(1, int.MaxValue)]
    public int? WorkstationCapacity { get; init; }

    [Range(1, int.MaxValue)]
    public int DailyRate { get; init; }
}