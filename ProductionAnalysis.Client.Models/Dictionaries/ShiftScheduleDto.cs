namespace ProductionAnalysis.Client.Models.Dictionaries;

public record ShiftScheduleDto
{
    public int Id { get; init; }
    public int ShiftId { get; init; }
    public int AuxiliaryOperationId { get; init; }
    public TimeOnly StartTime { get; init; }
}