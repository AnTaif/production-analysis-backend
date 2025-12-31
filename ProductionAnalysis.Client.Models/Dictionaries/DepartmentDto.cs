namespace ProductionAnalysis.Client.Models.Dictionaries;

public record DepartmentDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int EnterpriseId { get; init; }
}