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
        Template template,
        Dictionary<string, object>? formContext = null);
}

[RegisterScoped]
public class FormRowInitializer(
    IPaUnitOfWork unitOfWork,
    IProductContextExtractor productContextExtractor,
    IFormRowDataFactory formRowDataFactory
) : IFormRowInitializer
{
    public async Task<ICollection<FormRowData>> InitializeRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template,
        Dictionary<string, object>? formContext = null)
    {
        var indicators = ExtractIndicators(template);
        var additionalOperationsByIds = await LoadAdditionalOperationsAsync();
        var productContext = productContextExtractor.Extract(formContext);
        var sortedBreaks = schedules.OrderBy(s => s.StartTime).ToList();
        var shiftEndTime = CalculateShiftEndTime(shiftStartTime);

        var rows = new List<FormRowData>();
        var currentTime = shiftStartTime;
        var breakIndex = 0;
        short order = 1;

        while (currentTime < shiftEndTime)
        {
            var hourEnd = CalculateHourEnd(currentTime, shiftEndTime);
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);

            if (nextBreak != null && IsBreakInTimeRange(nextBreak, currentTime, hourEnd))
            {
                order = ProcessBreakInterval(
                    rows,
                    order,
                    hourEnd,
                    nextBreak,
                    additionalOperationsByIds,
                    indicators,
                    productContext,
                    ref breakIndex,
                    ref currentTime);
            }
            else
            {
                var workRow = formRowDataFactory.CreateWorkRow(
                    order++,
                    indicators.WorkTime,
                    indicators.Plan,
                    currentTime,
                    hourEnd,
                    productContext);

                rows.Add(workRow);
                currentTime = hourEnd;
            }
        }

        return rows;
    }

    private async Task<Dictionary<int, AdditionalOperationDto>> LoadAdditionalOperationsAsync()
    {
        var operations = await unitOfWork.Dictionaries.SelectAdditionalOperationsAsync();
        return operations.ToDictionary(ao => ao.Id);
    }

    private static InitializedIndicators ExtractIndicators(Template template)
    {
        return new InitializedIndicators
        {
            WorkTime = template.Indicators.Single(i => i.Id == ShiftConstants.WorktimeIndicatorId),
            Plan = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.PlanIndicatorId)
        };
    }

    private static ShiftScheduleDto? GetNextBreak(List<ShiftScheduleDto> sortedBreaks, int breakIndex)
    {
        return breakIndex < sortedBreaks.Count ? sortedBreaks[breakIndex] : null;
    }

    private static bool IsBreakInTimeRange(ShiftScheduleDto breakSchedule, TimeOnly rangeStart, TimeOnly rangeEnd)
    {
        return rangeStart <= breakSchedule.StartTime && breakSchedule.StartTime < rangeEnd;
    }

    private short ProcessBreakInterval(
        List<FormRowData> rows,
        short order,
        TimeOnly hourEnd,
        ShiftScheduleDto nextBreak,
        Dictionary<int, AdditionalOperationDto> additionalOperationsByIds,
        InitializedIndicators indicators,
        ProductContext? productContext,
        ref int breakIndex,
        ref TimeOnly currentTime)
    {
        // Создаем строку работы до перерыва
        if (currentTime < nextBreak.StartTime)
        {
            var workRowBeforeBreak = formRowDataFactory.CreateWorkRow(
                order++,
                indicators.WorkTime,
                indicators.Plan,
                currentTime,
                nextBreak.StartTime,
                productContext);

            rows.Add(workRowBeforeBreak);
        }

        // Создаем строку для обед/перерыв
        var breakMetaInfo = additionalOperationsByIds[nextBreak.AdditionalOperationId];
        var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

        var breakRow = formRowDataFactory.CreateBreakRow(
            order++,
            indicators.WorkTime,
            nextBreak.StartTime,
            breakEndTime,
            breakMetaInfo.Name,
            nextBreak.AdditionalOperationId);

        rows.Add(breakRow);

        currentTime = breakEndTime;
        breakIndex++;

        // Если после перерыва осталось время в этом часе, создаем строку работы
        if (currentTime < hourEnd)
        {
            var workRowAfterBreak = formRowDataFactory.CreateWorkRow(
                order++,
                indicators.WorkTime,
                indicators.Plan,
                currentTime,
                hourEnd,
                productContext);

            rows.Add(workRowAfterBreak);
            currentTime = hourEnd;
        }

        return order;
    }

    private static TimeOnly CalculateShiftEndTime(TimeOnly shiftStartTime)
    {
        return shiftStartTime
            .AddHours(ShiftConstants.ShiftDurationHours)
            .AddMinutes(ShiftConstants.ShiftDurationMinutes);
    }

    private static TimeOnly CalculateHourEnd(TimeOnly currentTime, TimeOnly shiftEndTime)
    {
        var hourEnd = currentTime.AddHours(1);
        return hourEnd > shiftEndTime ? shiftEndTime : hourEnd;
    }

    private record InitializedIndicators
    {
        public required Indicator WorkTime { get; init; }
        public Indicator? Plan { get; init; }
    }
}