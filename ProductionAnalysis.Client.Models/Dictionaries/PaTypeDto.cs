namespace ProductionAnalysis.Client.Models.Dictionaries;

public record PaTypeDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}