using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms;

public record CreateFormRequest
{
    public PaTypeDto PaType { get; init; }

    [Range(1, int.MaxValue)]
    public int ShiftId { get; init; }

    public ProductContextDto? Product { get; init; }

    public ICollection<ProductContextDto>? Products { get; init; }

    public OperationContextDto? Operation { get; init; }
}