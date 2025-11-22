using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class EmployeeDbo
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string FirstName { get; set; }
    
    [MaxLength(255)]
    public required string LastName { get; set; }
    
    [MaxLength(255)]
    public string? MiddleName { get; set; }
    
    [MaxLength(255)]
    public required string Position { get; set; }
    
    public int DepartmentId { get; set; }
}