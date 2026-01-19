using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record CreateEnterpriseRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }
}