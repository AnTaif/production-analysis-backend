namespace ProductionAnalysis.Client.Models.Dictionaries;

public record AdditionalOperationDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
}