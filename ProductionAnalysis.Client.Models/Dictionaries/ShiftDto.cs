namespace ProductionAnalysis.Client.Models.Dictionaries;

public record ShiftDto(
    int Id,
    string Name,
    TimeOnly StartTime
);