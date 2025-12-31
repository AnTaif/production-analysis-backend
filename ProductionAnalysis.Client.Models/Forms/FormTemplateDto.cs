namespace ProductionAnalysis.Client.Models.Forms;

public record FormTemplateDto
{
    public ICollection<FormFieldDto> TableColumns { get; init; } = new List<FormFieldDto>();
}