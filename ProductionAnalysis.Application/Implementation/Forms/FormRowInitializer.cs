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
        Dictionary<string, FormContextBase>? formContext = null);
}

[RegisterScoped]
public class FormRowInitializer(
    IPaUnitOfWork unitOfWork,
    IProductContextExtractor productContextExtractor,
    IFormRowDataFactory formRowDataFactory,
    ICumulativeValueCalculator cumulativeValueCalculator
) : IFormRowInitializer
{
    public async Task<ICollection<FormRowData>> InitializeRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template,
        Dictionary<string, FormContextBase>? formContext = null)
    {
        var indicators = ExtractIndicators(template);
        var additionalOperationsByIds = await LoadAdditionalOperationsAsync();
        var productContext = productContextExtractor.Extract(formContext);
        var sortedBreaks = schedules.OrderBy(s => s.StartTime).ToList();

        var totalWorkTime = TimeSpan.FromHours(ShiftConstants.ShiftDurationHours);

        var rows = new List<FormRowData>();
        var currentTime = shiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        short order = 1;

        while (elapsedWorkTime < totalWorkTime)
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;

            var workIntervalDuration = remainingWorkTime >= TimeSpan.FromHours(1)
                ? TimeSpan.FromHours(1)
                : remainingWorkTime;

            var workIntervalEndTime = currentTime.Add(workIntervalDuration);

            if (nextBreak != null && IsBreakInWorkInterval(nextBreak, currentTime, workIntervalEndTime))
            {
                order = ProcessBreakInterval(
                    rows,
                    order,
                    nextBreak,
                    additionalOperationsByIds,
                    indicators,
                    productContext,
                    ref breakIndex,
                    ref currentTime,
                    ref elapsedWorkTime);
            }
            else
            {
                var workRow = formRowDataFactory.CreateWorkRow(
                    order++,
                    indicators.WorkTime,
                    indicators.Plan,
                    currentTime,
                    workIntervalEndTime,
                    productContext);

                rows.Add(workRow);
                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workIntervalDuration);
            }
        }

        // Если чистое рабочее время закончилось, но остались перерывы - в цикле добавляем их в расписание
        while (breakIndex < sortedBreaks.Count)
        {
            var nextBreak = sortedBreaks[breakIndex];
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
            breakIndex++;
        }

        cumulativeValueCalculator.FillCumulativeValues(rows, template.Indicators);

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

    private static bool IsBreakInWorkInterval(ShiftScheduleDto breakSchedule, TimeOnly intervalStart,
        TimeOnly intervalEnd)
    {
        return intervalStart <= breakSchedule.StartTime && breakSchedule.StartTime < intervalEnd;
    }

    private short ProcessBreakInterval(
        List<FormRowData> rows,
        short order,
        ShiftScheduleDto nextBreak,
        Dictionary<int, AdditionalOperationDto> additionalOperationsByIds,
        InitializedIndicators indicators,
        ProductContext? productContext,
        ref int breakIndex,
        ref TimeOnly currentTime,
        ref TimeSpan elapsedWorkTime)
    {
        if (currentTime < nextBreak.StartTime)
        {
            var workDuration = nextBreak.StartTime - currentTime;
            var workRowBeforeBreak = formRowDataFactory.CreateWorkRow(
                order++,
                indicators.WorkTime,
                indicators.Plan,
                currentTime,
                nextBreak.StartTime,
                productContext);

            rows.Add(workRowBeforeBreak);

            elapsedWorkTime = elapsedWorkTime.Add(workDuration);
        }

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

        return order;
    }

    private record InitializedIndicators
    {
        public required Indicator WorkTime { get; init; }
        public Indicator? Plan { get; init; }
    }
}