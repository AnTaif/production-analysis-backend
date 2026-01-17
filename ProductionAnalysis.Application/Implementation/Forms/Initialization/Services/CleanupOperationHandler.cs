using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Constants;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface ICleanupOperationHandler
{
    bool IsCleanupOperation(int auxiliaryOperationId);
    ICollection<ShiftScheduleDto> FilterOutCleanup(ICollection<ShiftScheduleDto> schedules);
    ICollection<FormRowData> FilterOutCleanup(ICollection<FormRowData> rows);

    FormRowData? CreateCleanupRow(
        TimeOnly startTime,
        short order,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators);
}

[RegisterScoped]
public class CleanupOperationHandler(IFormRowDataFactory formRowDataFactory) : ICleanupOperationHandler
{
    public bool IsCleanupOperation(int auxiliaryOperationId)
    {
        return auxiliaryOperationId == AuxiliaryOperationIds.Cleanup;
    }

    public ICollection<ShiftScheduleDto> FilterOutCleanup(ICollection<ShiftScheduleDto> schedules)
    {
        return schedules
            .Where(s => !IsCleanupOperation(s.AuxiliaryOperationId))
            .ToList();
    }

    public ICollection<FormRowData> FilterOutCleanup(ICollection<FormRowData> rows)
    {
        return rows
            .Where(r => !r.IsAuxiliaryOperation || r.AuxiliaryOperationId != AuxiliaryOperationIds.Cleanup)
            .ToList();
    }

    public FormRowData? CreateCleanupRow(
        TimeOnly startTime,
        short order,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators)
    {
        if (!auxiliaryOperations.TryGetValue(AuxiliaryOperationIds.Cleanup, out var cleanupOperation))
            return null;

        var endTime = startTime.Add(cleanupOperation.Duration);

        return formRowDataFactory.CreateBreakRow(
            order,
            indicators.WorkTime,
            startTime,
            endTime,
            cleanupOperation.Name,
            AuxiliaryOperationIds.Cleanup,
            null);
    }
}