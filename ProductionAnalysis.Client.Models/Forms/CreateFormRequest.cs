using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms;

public record CreateFormRequest
{
    public PaTypeDto PaType { get; init; }

    [Range(1, int.MaxValue)]
    public int ShiftId { get; init; }

    [Range(1, int.MaxValue)]
    public int AssigneeId { get; init; }

    public DateTime FormDate { get; init; }

    public ProductContextRequest? Product { get; init; }

    public ICollection<ProductContextRequest>? Products { get; init; }

    public OperationOrProductContextRequest? OperationOrProduct { get; init; }
}