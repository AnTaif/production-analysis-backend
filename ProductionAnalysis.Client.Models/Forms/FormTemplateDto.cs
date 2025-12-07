namespace ProductionAnalysis.Client.Models.Forms;

public record FormTemplateDto(
    ICollection<FormFieldDto> ContextFields,
    ICollection<FormFieldDto> TableColumns
);