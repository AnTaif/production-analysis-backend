namespace ProductionAnalysis.Client.Models.Dictionaries;

public record ShiftDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TimeOnly StartTime { get; init; }
}