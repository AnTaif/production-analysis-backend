using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Client.Models.Forms;

public record FormShortDto
{
    public int Id { get; init; }
    public PaTypeDto PaType { get; init; }
    public FormStatus Status { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime UpdateDate { get; init; }
    public int DepartmentId { get; init; }
    public required EmployeeDto Creator { get; init; }
    public required EmployeeDto Executor { get; init; }
}