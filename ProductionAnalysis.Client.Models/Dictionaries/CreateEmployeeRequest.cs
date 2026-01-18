using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record CreateEmployeeRequest
{
    [Required]
    [MaxLength(255)]
    public required string FirstName { get; init; }

    [Required]
    [MaxLength(255)]
    public required string LastName { get; init; }

    [MaxLength(255)]
    public string? MiddleName { get; init; }

    [Required]
    [MaxLength(255)]
    public required string Position { get; init; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; init; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; init; }
}