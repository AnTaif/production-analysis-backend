namespace ProductionAnalysis.Client.Models.Dictionaries;

public record ShiftDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public TimeOnly StartTime { get; init; }
}