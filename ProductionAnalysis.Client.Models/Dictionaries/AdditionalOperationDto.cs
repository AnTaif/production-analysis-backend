namespace ProductionAnalysis.Client.Models.Dictionaries;

public record AdditionalOperationDto(
    int Id,
    string Name,
    TimeSpan Duration
);