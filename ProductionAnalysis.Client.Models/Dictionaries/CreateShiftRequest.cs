using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record CreateShiftRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }

    [Required]
    public TimeOnly StartTime { get; init; }
}