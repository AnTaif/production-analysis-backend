using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IBreakProcessor
{
    /// <summary>
    ///     Проверяет, нужно ли вставить перерыв в рабочий интервал
    /// </summary>
    bool ShouldInsertBreak(TimeOnly currentTime, ShiftScheduleDto breakSchedule, TimeOnly intervalEnd);

    /// <summary>
    ///     Обрабатывает перерыв в рабочем интервале
    /// </summary>
    BreakProcessingResult ProcessBreak(
        ShiftScheduleDto breakSchedule,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext,
        ref short order,
        ref TimeOnly currentTime,
        ref TimeSpan elapsedWorkTime);

    /// <summary>
    ///     Обрабатывает оставшиеся перерывы после завершения рабочего времени
    /// </summary>
    ICollection<FormRowData> ProcessRemainingBreaks(
        ICollection<ShiftScheduleDto> remainingBreaks,
        short startOrder,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext = null);
}

[RegisterScoped]
public class BreakProcessor(IFormRowDataFactory formRowDataFactory) : IBreakProcessor
{
    public bool ShouldInsertBreak(TimeOnly currentTime, ShiftScheduleDto breakSchedule, TimeOnly intervalEnd)
    {
        return currentTime <= breakSchedule.StartTime && breakSchedule.StartTime < intervalEnd;
    }

    public BreakProcessingResult ProcessBreak(
        ShiftScheduleDto breakSchedule,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext,
        ref short order,
        ref TimeOnly currentTime,
        ref TimeSpan elapsedWorkTime)
    {
        var rows = new List<FormRowData>();

        // Если есть рабочее время до перерыва
        if (currentTime < breakSchedule.StartTime)
        {
            var workDuration = breakSchedule.StartTime - currentTime;
            var workRow = formRowDataFactory.CreateWorkRow(
                order++,
                indicators.WorkTime!,
                indicators.Plan,
                currentTime,
                breakSchedule.StartTime,
                productContext);

            rows.Add(workRow);
            elapsedWorkTime = elapsedWorkTime.Add(workDuration);
        }

        // Создаем строку перерыва
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
        currentTime = breakEndTime;

        return new BreakProcessingResult
        {
            Rows = rows,
            NextBreakIndex = 1 // Инкрементируется в вызывающем коде
        };
    }

    public ICollection<FormRowData> ProcessRemainingBreaks(
        ICollection<ShiftScheduleDto> remainingBreaks,
        short startOrder,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext = null)
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