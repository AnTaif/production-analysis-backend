using System.ComponentModel.DataAnnotations;
using ProductionAnalysis.Client.Models.Forms.FormsCreation;

namespace ProductionAnalysis.Client.Models.Forms;

public record CreateFormRequest
{
    [Range(1, int.MaxValue)]
    public int PaTypeId { get; init; }

    [Range(1, int.MaxValue)]
    public int ShiftId { get; init; }

    public required Dictionary<string, CreateFormRequestContextBase> Context { get; init; }
}