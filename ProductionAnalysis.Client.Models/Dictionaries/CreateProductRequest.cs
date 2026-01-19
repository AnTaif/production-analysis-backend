using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record CreateProductRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }

    [Range(1, int.MaxValue)]
    public int TactTimeInSeconds { get; init; }

    [Range(1, int.MaxValue)]
    public int EnterpriseId { get; init; }
}