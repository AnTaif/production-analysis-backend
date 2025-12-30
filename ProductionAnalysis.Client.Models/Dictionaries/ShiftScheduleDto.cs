namespace ProductionAnalysis.Client.Models.Dictionaries;

public record ShiftScheduleDto(
    int Id,
    int ShiftId,
    int AdditionalOperationId,
    TimeOnly StartTime
);