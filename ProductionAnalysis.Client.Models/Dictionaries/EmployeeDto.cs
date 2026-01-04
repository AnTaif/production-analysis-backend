namespace ProductionAnalysis.Client.Models.Dictionaries;

public record EmployeeDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public int DepartmentId { get; init; }
    public Guid? UserId { get; init; }
}