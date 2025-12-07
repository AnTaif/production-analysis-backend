namespace ProductionAnalysis.Client.Models.Forms;

public record SearchFormsFilterDto(
    int? DepartmentId,
    FormStatus? Status,
    int PageNumber = 1,
    int PageSize = 10
);