using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms;

public record UpdateFormRowRequest
{
    [Required]
    public Dictionary<int, object> Values { get; init; } = new(); // Key - IndicatorId, Value - новое значение
}