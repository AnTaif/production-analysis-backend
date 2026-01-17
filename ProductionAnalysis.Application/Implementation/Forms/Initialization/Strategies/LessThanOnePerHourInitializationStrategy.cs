using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

public class LessThanOnePerHourInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager,
    IOperationService operationService,
    ICleanupOperationHandler cleanupHandler
)
    : OperationOrProductInitializationStrategyBase(operationService), IRowInitializationStrategy
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerHour;
    }

    public override async Task<ICollection<FormRowData>> InitializeAsync(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.RequireContext<OperationOrProductContext>(FormContextAccessor
                .OperationOrProductContextKey);
        var relatedOperations = await GetRelatedOperationsAsync(operationContext);

        var totalWorkTime = shiftTimeManager.GetTotalWorkTime();
        var cycleDuration = OperationService.CalculateCycleDuration(relatedOperations);

        var rows = new List<FormRowData>();
        var currentTime = context.ShiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        short order = 1;

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;

            var timeUntilBreak = nextBreak != null
                ? (nextBreak.StartTime - currentTime).TotalSeconds
                : remainingWorkTime.TotalSeconds;

            if (ShouldProcessBreak(nextBreak, timeUntilBreak, cycleDuration))
            {
                var breakMetaInfo = context.AuxiliaryOperations[nextBreak!.AuxiliaryOperationId];
                var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

                var breakRow = formRowDataFactory.CreateBreakRow(
                    order++,
                    context.Indicators.WorkTime,
                    nextBreak.StartTime,
                    breakEndTime,
                    breakMetaInfo.Name,
                    nextBreak.AuxiliaryOperationId,
                    null);

                rows.Add(breakRow);
                currentTime = breakEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(breakMetaInfo.Duration);
                breakIndex++;
            }
            else if (remainingWorkTime.TotalSeconds >= cycleDuration)
            {
                var cycleEndTime = currentTime.Add(TimeSpan.FromSeconds(cycleDuration));

                var cycleRows = formRowDataFactory.CreateOperationCycleRows(
                    ref order,
                    context.Indicators.WorkTime!,
                    context.Indicators.Plan,
                    context.Indicators.OperationName,
                    context.Indicators.OperationTime,
                    currentTime,
                    cycleEndTime,
                    relatedOperations);

                rows.AddRange(cycleRows);
                currentTime = cycleEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(TimeSpan.FromSeconds(cycleDuration));
            }
            else
            {
                var cycleEndTime = currentTime.Add(remainingWorkTime);

                var cycleRows = formRowDataFactory.CreateOperationCycleRows(
                    ref order,
                    context.Indicators.WorkTime!,
                    context.Indicators.Plan,
                    context.Indicators.OperationName,
                    context.Indicators.OperationTime,
                    currentTime,
                    cycleEndTime,
                    relatedOperations);

                rows.AddRange(cycleRows);
                break;
            }
        }

        var remainingBreaks = cleanupHandler.FilterOutCleanup(
            context.SortedSchedules.Skip(breakIndex).ToList());

        if (remainingBreaks.Count > 0)
        {
            var remainingBreakRows = breakProcessor.ProcessRemainingBreaks(
                remainingBreaks,
                order,
                context.AuxiliaryOperations,
                context.Indicators,
                null);

            rows.AddRange(remainingBreakRows);

            if (remainingBreakRows.Count > 0)
            {
                var lastBreak = remainingBreaks.Last();
                if (context.AuxiliaryOperations.TryGetValue(lastBreak.AuxiliaryOperationId, out var lastBreakOp))
                {
                    currentTime = lastBreak.StartTime.Add(lastBreakOp.Duration);
                }
            }
        }

        var cleanupRow = cleanupHandler.CreateCleanupRow(
            currentTime,
            GetNextOrder(rows),
            context.AuxiliaryOperations,
            context.Indicators);

        if (cleanupRow != null)
        {
            rows.Add(cleanupRow);
        }

        return rows;
    }

    private bool ShouldProcessBreak(ShiftScheduleDto? nextBreak, double timeUntilBreak, double cycleDuration)
    {
        if (nextBreak == null)
            return false;

        if (cleanupHandler.IsCleanupOperation(nextBreak.AuxiliaryOperationId))
            return false;

        return timeUntilBreak > 0 && timeUntilBreak < cycleDuration;
    }

    private static short GetNextOrder(List<FormRowData> rows)
    {
        return (short)(rows.Count > 0 ? rows.Max(r => r.Order) + 1 : 1);
    }
}