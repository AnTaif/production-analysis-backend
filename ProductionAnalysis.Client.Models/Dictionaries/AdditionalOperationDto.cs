namespace ProductionAnalysis.Client.Models.Dictionaries;

public record AdditionalOperationDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public TimeSpan Duration { get; init; }
}