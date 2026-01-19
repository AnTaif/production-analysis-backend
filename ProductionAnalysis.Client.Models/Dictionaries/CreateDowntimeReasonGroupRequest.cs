using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record CreateDowntimeReasonGroupRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }

    [Required]
    [MaxLength(300)]
    public required string Description { get; init; }
}