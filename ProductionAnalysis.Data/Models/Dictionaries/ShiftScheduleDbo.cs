namespace ProductionAnalysis.Data.Models.Dictionaries;

public class ShiftScheduleDbo
{
    public int Id { get; set; }

    public int ShiftId { get; set; }

    public int AuxiliaryOperationId { get; set; }

    public TimeOnly StartTime { get; set; }

    public ShiftDbo Shift { get; set; } = null!;

    public AuxiliaryOperationDbo AuxiliaryOperation { get; set; } = null!;
}