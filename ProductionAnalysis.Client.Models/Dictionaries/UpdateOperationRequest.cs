using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Dictionaries;

public record UpdateOperationRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }

    [Range(1, int.MaxValue)]
    public int? DurationInSeconds { get; init; }

    [Required]
    public OperationBasedOnType BasedOnType { get; init; }

    [Range(1, int.MaxValue)]
    public int? BasedOperationId { get; init; }

    [Range(1, int.MaxValue)]
    public int? BasedProductId { get; init; }
}