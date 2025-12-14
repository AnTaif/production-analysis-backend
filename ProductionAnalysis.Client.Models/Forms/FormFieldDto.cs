namespace ProductionAnalysis.Client.Models.Forms;

public record FormFieldDto(
    int Id,
    string Name,
    string InputType,
    string? InputSelector,
    string? ValueType
);