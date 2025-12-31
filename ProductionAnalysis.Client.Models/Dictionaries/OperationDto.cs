namespace ProductionAnalysis.Client.Models.Dictionaries;

public record OperationDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TimeSpan? Duration { get; init; }
    public OperationBasedOnType BasedOnType { get; init; }
    public int? BasedOperationId { get; init; }
    public int? BasedProductId { get; init; }
}