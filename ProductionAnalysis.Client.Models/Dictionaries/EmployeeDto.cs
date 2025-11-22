namespace ProductionAnalysis.Client.Models.Dictionaries;

public record EmployeeDto(
    int Id,
    string FullName,
    string Position,
    int DepartmentId
);