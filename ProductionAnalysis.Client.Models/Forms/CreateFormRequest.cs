using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms;

public record CreateFormRequest
{
    [Range(1, int.MaxValue)]
    public int PaTypeId { get; init; }

    [Range(1, int.MaxValue)]
    public int ShiftId { get; init; }

    public ProductContextDto? Product { get; init; }
    public OperationContextDto? Operation { get; init; }
}