namespace ProductionAnalysis.Client.Models.Forms;

public record FormShortDto(
    int Id,
    int PaTypeId,
    FormStatus Status,
    DateTime CreationDate,
    DateTime UpdateDate
);