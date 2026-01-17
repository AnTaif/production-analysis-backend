using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IBreakProcessor
{
    bool ShouldInsertBreak(TimeOnly currentTime, ShiftScheduleDto breakSchedule, TimeOnly intervalEnd);

    BreakProcessingResult ProcessBreak(
        ShiftScheduleDto breakSchedule,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext,
        ref short order,
        ref TimeOnly currentTime,
        ref TimeSpan elapsedWorkTime,
        bool isFirst = false);

    ICollection<FormRowData> ProcessRemainingBreaks(
        ICollection<ShiftScheduleDto> remainingBreaks,
        short startOrder,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext = null,
        bool isLast = true);
}

[RegisterScoped]
public class BreakProcessor(IFormRowDataFactory formRowDataFactory) : IBreakProcessor
{
    public bool ShouldInsertBreak(TimeOnly currentTime, ShiftScheduleDto breakSchedule, TimeOnly intervalEnd)
    {
        if (intervalEnd < currentTime)
        {
            return breakSchedule.StartTime >= currentTime || breakSchedule.StartTime < intervalEnd;
        }

        return currentTime <= breakSchedule.StartTime && breakSchedule.StartTime < intervalEnd;
    }

    public BreakProcessingResult ProcessBreak(
        ShiftScheduleDto breakSchedule,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext,
        ref short order,
        ref TimeOnly currentTime,
        ref TimeSpan elapsedWorkTime,
        bool isFirst = false)
    {
        var rows = new List<FormRowData>();

        if (currentTime < breakSchedule.StartTime)
        {
            var workDuration = breakSchedule.StartTime - currentTime;

            if (productContext is not null)
            {
                var workRow = formRowDataFactory.CreateWorkRow(
                    order++,
                    indicators.WorkTime!,
                    indicators.Plan,
                    currentTime,
                    breakSchedule.StartTime,
                    productContext);

                rows.Add(workRow);
            }

            elapsedWorkTime = elapsedWorkTime.Add(workDuration);
            currentTime = breakSchedule.StartTime;
        }

        var breakMetaInfo = auxiliaryOperations[breakSchedule.AuxiliaryOperationId];
        var breakEndTime = breakSchedule.StartTime.Add(breakMetaInfo.Duration);
        var operationProductContext = isFirst && currentTime >= breakSchedule.StartTime ? null : productContext;

        var breakRow = formRowDataFactory.CreateBreakRow(
            order++,
            indicators.WorkTime,
            breakSchedule.StartTime,
            breakEndTime,
            breakMetaInfo.Name,
            breakSchedule.AuxiliaryOperationId,
            operationProductContext);

        rows.Add(breakRow);
        currentTime = breakEndTime;

        return new BreakProcessingResult
        {
            Rows = rows,
            NextBreakIndex = 1
        };
    }

    public ICollection<FormRowData> ProcessRemainingBreaks(
        ICollection<ShiftScheduleDto> remainingBreaks,
        short startOrder,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext = null,
        bool isLast = true)
    {
        var rows = new List<FormRowData>();
        var order = startOrder;

        foreach (var breakSchedule in remainingBreaks)
        {
            var breakMetaInfo = auxiliaryOperations[breakSchedule.AuxiliaryOperationId];
            var breakEndTime = breakSchedule.StartTime.Add(breakMetaInfo.Duration);

            var breakRow = formRowDataFactory.CreateBreakRow(
                order++,
                indicators.WorkTime,
                breakSchedule.StartTime,
                breakEndTime,
                breakMetaInfo.Name,
                breakSchedule.AuxiliaryOperationId,
                productContext);

            rows.Add(breakRow);
        }

        return rows;
    }
}