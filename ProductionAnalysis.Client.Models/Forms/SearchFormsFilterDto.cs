using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms;

public record SearchFormsFilterDto
{
    [Range(1, int.MaxValue)] public int? DepartmentId { get; init; }

    public FormStatus? Status { get; init; }

    [Range(1, int.MaxValue)] public int PageNumber { get; init; } = 1;

    [Range(1, 100)] public int PageSize { get; init; } = 10;
}