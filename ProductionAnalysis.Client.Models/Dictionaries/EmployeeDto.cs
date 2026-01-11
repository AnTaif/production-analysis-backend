namespace ProductionAnalysis.Client.Models.Dictionaries;

public record EmployeeDto
{
    public int Id { get; init; }
    public required string FullName { get; init; }
    public required string Position { get; init; }
    public int DepartmentId { get; init; }
    public Guid? UserId { get; init; }
}