namespace ProductionAnalysis.Client.Models.Dictionaries;

public record OperationDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public TimeSpan? Duration { get; init; }
    public OperationBasedOnType BasedOnType { get; init; }
    public int? BasedOperationId { get; init; }
    public int? BasedProductId { get; init; }
    public ICollection<OperationDto> SubOperations { get; init; } = new List<OperationDto>();
}