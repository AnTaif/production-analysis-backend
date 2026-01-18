namespace ProductionAnalysis.Client.Models.Dictionaries;

public record PositionDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Role { get; init; }
}