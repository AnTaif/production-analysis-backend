using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowGenerator
{
    Task<ICollection<FormRowData>> GenerateRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template);
}

[RegisterScoped]
public class FormRowGenerator(
    IPaUnitOfWork unitOfWork
) : IFormRowGenerator
{
    private const int ShiftDurationHours = 8;
    private const int ShiftDurationMinutes = 40; // 8 часов 40 минут для смены 

    private const int WorktimeIndicatorId = 16;
    private const int OperationNameIndicatorId = 9;

    public async Task<ICollection<FormRowData>> GenerateRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template)
    {
        var indicators = ExtractIndicators(template);
        var breaks = await LoadBreaksAsync();
        var sortedBreakSchedules = schedules.OrderBy(s => s.StartTime).ToList();
        var shiftEndTime = CalculateShiftEndTime(shiftStartTime);

        var rows = new List<FormRowData>();
        short order = 1;
        var currentTime = shiftStartTime;
        var breakIndex = 0;

        while (currentTime < shiftEndTime)
        {
            var hourEnd = CalculateHourEnd(currentTime, shiftEndTime);
            var nextBreak = GetNextBreak(sortedBreakSchedules, breakIndex);

            if (HasBreakInTimeRange(nextBreak, currentTime, hourEnd))
            {
                var (processedRows, newOrder, newTime, newBreakIndex) = ProcessBreak(
                    currentTime,
                    hourEnd,
                    nextBreak!,
                    indicators,
                    breaks,
                    order,
                    breakIndex);

                rows.AddRange(processedRows);
                order = newOrder;
                currentTime = newTime;
                breakIndex = newBreakIndex;
            }
            else
            {
                var workRow = CreateWorkRow(order++, indicators.WorkTime, currentTime, hourEnd);
                rows.Add(workRow);
                currentTime = hourEnd;
            }
        }

        return rows;
    }

    private static (Indicator WorkTime, Indicator OperationName) ExtractIndicators(Template template)
    {
        var workTimeIndicator = template.Indicators.Single(i => i.Id == WorktimeIndicatorId);
        var operationNameIndicator = template.Indicators.Single(i => i.Id == OperationNameIndicatorId);
        return (workTimeIndicator, operationNameIndicator);
    }

    private async Task<Dictionary<int, AdditionalOperationDto>> LoadBreaksAsync()
    {
        var operations = await unitOfWork.Dictionaries.SelectAdditionalOperationsAsync();
        return operations.ToDictionary(ao => ao.Id);
    }

    private static TimeOnly CalculateShiftEndTime(TimeOnly shiftStartTime)
    {
        return shiftStartTime.AddHours(ShiftDurationHours).AddMinutes(ShiftDurationMinutes);
    }

    private static TimeOnly CalculateHourEnd(TimeOnly currentTime, TimeOnly shiftEndTime)
    {
        var hourEnd = currentTime.AddHours(1);
        return hourEnd > shiftEndTime ? shiftEndTime : hourEnd;
    }

    private static ShiftScheduleDto? GetNextBreak(List<ShiftScheduleDto> sortedBreakSchedules, int breakIndex)
    {
        return breakIndex < sortedBreakSchedules.Count ? sortedBreakSchedules[breakIndex] : null;
    }

    private static bool HasBreakInTimeRange(ShiftScheduleDto? nextBreak, TimeOnly rangeStart, TimeOnly rangeEnd)
    {
        return nextBreak != null && rangeStart <= nextBreak.StartTime && nextBreak.StartTime < rangeEnd;
    }

    private static (List<FormRowData> rows, short order, TimeOnly currentTime, int breakIndex) ProcessBreak(
        TimeOnly currentTime,
        TimeOnly hourEnd,
        ShiftScheduleDto nextBreak,
        (Indicator WorkTime, Indicator OperationName) indicators,
        Dictionary<int, AdditionalOperationDto> additionalOperations,
        short order,
        int breakIndex)
    {
        var rows = new List<FormRowData>();

        // note: если есть время до перерыва - создаем строку работы на это время
        if (currentTime < nextBreak.StartTime)
        {
            var workRowBeforeBreak = CreateWorkRow(order++, indicators.WorkTime, currentTime, nextBreak.StartTime);
            rows.Add(workRowBeforeBreak);
        }

        var breakRow = CreateBreakRow(
            order++,
            nextBreak,
            indicators,
            additionalOperations);

        rows.Add(breakRow);

        var breakEndTime = CalculateBreakEndTime(nextBreak, additionalOperations);
        var newCurrentTime = breakEndTime;
        var newBreakIndex = breakIndex + 1;

        // Если после перерыва осталось время в этом часе, создаем строку работы
        if (newCurrentTime < hourEnd)
        {
            var workRowAfterBreak = CreateWorkRow(order++, indicators.WorkTime, newCurrentTime, hourEnd);
            rows.Add(workRowAfterBreak);
            newCurrentTime = hourEnd;
        }

        return (rows, order, newCurrentTime, newBreakIndex);
    }

    private static TimeOnly CalculateBreakEndTime(
        ShiftScheduleDto breakSchedule,
        Dictionary<int, AdditionalOperationDto> additionalOperations)
    {
        var breakMetaInfo = additionalOperations[breakSchedule.AdditionalOperationId];
        return breakSchedule.StartTime.Add(breakMetaInfo.Duration);
    }

    private static FormRowData CreateBreakRow(
        short order,
        ShiftScheduleDto breakSchedule,
        (Indicator WorkTime, Indicator OperationName) indicators,
        Dictionary<int, AdditionalOperationDto> additionalOperations)
    {
        var breakMetaInfo = additionalOperations[breakSchedule.AdditionalOperationId];
        var breakEndTime = breakSchedule.StartTime.Add(breakMetaInfo.Duration);

        var breakRowValues = new List<FormRowValueData>
        {
            CreateFormRowValueData(indicators.WorkTime, FormatTimeRange(breakSchedule.StartTime, breakEndTime)),
            CreateFormRowValueData(indicators.OperationName, breakMetaInfo.Name)
        };

        return new FormRowData
        {
            Order = order,
            IsAdditionalOperation = true,
            AdditionalOperationId = breakSchedule.AdditionalOperationId,
            Values = breakRowValues
        };
    }

    private static FormRowData CreateWorkRow(short order, Indicator workTimeIndicator, TimeOnly startTime,
        TimeOnly endTime)
    {
        return new FormRowData
        {
            Order = order,
            IsAdditionalOperation = false,
            Values = [CreateFormRowValueData(workTimeIndicator, FormatTimeRange(startTime, endTime))]
        };
    }

    private static FormRowValueData CreateFormRowValueData(Indicator indicator, string value) =>
        new()
        {
            IndicatorId = indicator.Id,
            Value = value
        };

    private static string FormatTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        return $"{startTime:HH:mm}-{endTime:HH:mm}";
    }
}