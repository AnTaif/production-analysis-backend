namespace ProductionAnalysis.Client.Models.Dictionaries;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TimeSpan TactTime { get; init; }
    public int EnterpriseId { get; init; }
}