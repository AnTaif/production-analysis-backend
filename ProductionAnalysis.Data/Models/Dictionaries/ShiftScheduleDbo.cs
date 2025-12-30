namespace ProductionAnalysis.Data.Models.Dictionaries;

public class ShiftScheduleDbo
{
    public int Id { get; set; }

    public int ShiftId { get; set; }

    public int AdditionalOperationId { get; set; }

    public TimeOnly StartTime { get; set; }

    public ShiftDbo Shift { get; set; } = null!;

    public AdditionalOperationDbo AdditionalOperation { get; set; } = null!;
}