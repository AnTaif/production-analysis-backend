namespace ProductionAnalysis.Client.Models.Forms.FormsCreation;

public class CreateFormRequestProductInfo : CreateFormRequestInfoBase
{
    public int ProductId { get; set; }

    public int? CycleTime { get; set; }

    public int? WorkstationCapacity { get; set; }

    public int DailyRate { get; set; }
}