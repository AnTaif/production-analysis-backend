namespace ProductionAnalysis.Client.Models.Dictionaries;

public record ProductDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public TimeSpan TactTime { get; init; }
    public int EnterpriseId { get; init; }
}