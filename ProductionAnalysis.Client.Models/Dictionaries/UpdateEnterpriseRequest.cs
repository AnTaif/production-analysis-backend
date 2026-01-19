using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record UpdateEnterpriseRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }
}