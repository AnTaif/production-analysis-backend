using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record UpdatePositionRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }

    [MaxLength(255)]
    public string? Role { get; init; }
}