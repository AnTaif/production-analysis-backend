namespace ProductionAnalysis.Client.Models.Forms;

public record FormDto(
    int Id,
    int PaTypeId,
    FormStatus Status,
    DateTime CreationDate,
    DateTime UpdateDate,
    Dictionary<string, object> Context,
    ICollection<FormRowDto> Rows,
    FormTemplateDto Template
);