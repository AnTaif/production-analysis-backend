using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowInitializer
{
    Task<ICollection<FormRowData>> InitializeRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template);
}

[RegisterScoped]
public class FormRowInitializer(
    IPaUnitOfWork unitOfWork
) : IFormRowInitializer
{
    private const int ShiftDurationHours = 8;
    private const int ShiftDurationMinutes = 40; // 8 часов 40 минут для смены 

    private const int WorktimeIndicatorId = 16;
    private const int OperationNameIndicatorId = 9;

    public async Task<ICollection<FormRowData>> InitializeRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template)
    {
        var rows = new List<FormRowData>();
        short order = 1;

        var workTimeIndicator = template.Indicators.Single(i => i.Id == WorktimeIndicatorId);
        var operationNameIndicator = template.Indicators.Single(i => i.Id == OperationNameIndicatorId);

        var additionalOperationsByIds =
            (await unitOfWork.Dictionaries.SelectAdditionalOperationsAsync()).ToDictionary(ao => ao.Id);

        var sortedBreaks = schedules.OrderBy(s => s.StartTime).ToList();

        var shiftEndTime = shiftStartTime.AddHours(ShiftDurationHours).AddMinutes(ShiftDurationMinutes);

        var currentTime = shiftStartTime;
        var breakIndex = 0;

        while (currentTime < shiftEndTime)
        {
            // Определяем конец текущего часа
            var hourEnd = currentTime.AddHours(1);
            if (hourEnd > shiftEndTime)
            {
                hourEnd = shiftEndTime;
            }

            // Проверяем, есть ли обед/перерыв в текущем интервале
            var nextBreak = breakIndex < sortedBreaks.Count
                ? sortedBreaks[breakIndex]
                : null;

            if (nextBreak != null && IsBreakInCurrentTimeRange(nextBreak, currentTime, hourEnd))
            {
                // Создаем строку работы до перерыва
                if (currentTime < nextBreak.StartTime)
                {
                    var workRowBeforeBreak = CreateFormRowData(
                        order++,
                        false,
                        workTimeIndicator,
                        FormatTimeRangeValue(currentTime, nextBreak.StartTime)
                    );

                    rows.Add(workRowBeforeBreak);
                }

                // Создаем строку для обед/перерыв
                var breakMetaInfo = additionalOperationsByIds[nextBreak.AdditionalOperationId];

                var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);
                var breakRowValues = new List<FormRowValueData>
                {
                    CreateFormRowValueData(workTimeIndicator,
                        FormatTimeRangeValue(nextBreak.StartTime, breakEndTime)),

                    CreateFormRowValueData(operationNameIndicator, breakMetaInfo.Name)
                };

                var breakRow = CreateFormRowData(
                    order++,
                    true,
                    nextBreak.AdditionalOperationId,
                    breakRowValues
                );

                rows.Add(breakRow);

                // Продолжаем с времени окончания перерыва
                currentTime = breakEndTime;

                breakIndex++;

                // Если после перерыва осталось время в этом часе, создаем строку работы
                if (currentTime < hourEnd)
                {
                    var workRowAfterBreak = CreateFormRowData(
                        order++,
                        false,
                        workTimeIndicator,
                        FormatTimeRangeValue(currentTime, hourEnd)
                    );
                    rows.Add(workRowAfterBreak);
                    currentTime = hourEnd;
                }
            }
            else
            {
                // Обычный час работы без перерывов
                var workRow = CreateFormRowData(
                    order++,
                    false,
                    workTimeIndicator,
                    FormatTimeRangeValue(currentTime, hourEnd)
                );

                rows.Add(workRow);
                currentTime = hourEnd;
            }
        }

        return rows;
    }

    private static bool IsBreakInCurrentTimeRange(ShiftScheduleDto nextBreak, TimeOnly rangeStart, TimeOnly rangeEnd)
    {
        return rangeStart <= nextBreak.StartTime && nextBreak.StartTime < rangeEnd;
    }

    private static FormRowData CreateFormRowData(
        short order,
        bool isAdditionalOperation,
        int additionalOperationId,
        ICollection<FormRowValueData> values)
    {
        return new FormRowData
        {
            Order = order,
            IsAdditionalOperation = isAdditionalOperation,
            AdditionalOperationId = additionalOperationId,
            Values = values
        };
    }

    private static FormRowData CreateFormRowData(
        short order,
        bool isAdditionalOperation,
        Indicator indicator,
        string value)
    {
        return new FormRowData
        {
            Order = order,
            IsAdditionalOperation = isAdditionalOperation,
            Values = [CreateFormRowValueData(indicator, value)]
        };
    }

    private static FormRowValueData CreateFormRowValueData(Indicator indicator, string value) =>
        new()
        {
            IndicatorId = indicator.Id,
            Value = value
        };

    private static string FormatTimeRangeValue(TimeOnly startTime, TimeOnly endTime)
    {
        return $"{startTime:HH:mm}-{endTime:HH:mm}";
    }
}