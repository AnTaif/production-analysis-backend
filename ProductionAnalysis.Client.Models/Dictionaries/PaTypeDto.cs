namespace ProductionAnalysis.Client.Models.Dictionaries;

public record PaTypeDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}