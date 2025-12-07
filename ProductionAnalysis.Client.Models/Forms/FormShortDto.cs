namespace ProductionAnalysis.Client.Models.Forms;

public record FormShortDto(
    Guid Id,
    int PaTypeId,
    FormStatus Status,
    DateTime CreationDate,
    DateTime UpdateDate
);