namespace ProductionAnalysis.Client.Models.Forms;

public record SearchFormsFilterDto(
    int? DepartmentId,
    FormStatus? Status
);