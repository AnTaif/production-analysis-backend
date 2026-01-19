using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record CreateAuxiliaryOperationRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }

    [Range(1, int.MaxValue)]
    public int DurationInSeconds { get; init; }
}