namespace ProductionAnalysis.Client.Models.Forms;

public record FormFieldDto(
    int Id,
    string Name,
    FormFieldInputType InputType,
    string? InputSelector,
    string? ValueType
);