using Core.Time;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

public class LessThanOnePerShiftInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager,
    IOperationService operationService,
    ICleanupOperationHandler cleanupHandler,
    IFormRowEndTimeExtractor endTimeExtractor
)
    : OperationOrProductInitializationStrategyBase(operationService, cleanupHandler, endTimeExtractor, breakProcessor),
        IRowInitializationStrategy
{
    private readonly IBreakProcessor breakProcessor = breakProcessor;

    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerShift;
    }

    protected override ICollection<FormRowData> InitializeRows(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.Require<OperationOrProductContext>(FormContextAccessor.OperationOrProductContextKey);
        var relatedOperations = GetRelatedOperations(operationContext, context.AllOperations);

        var workTimeTracker = new WorkTimeTracker(shiftTimeManager);
        var shiftStartMinutes = context.ShiftStartTime.TotalMinutes();

        var rows = new List<FormRowData>();
        var currentTime = context.ShiftStartTime;
        var breakIndex = 0;
        short order = 1;
        var operationIndex = 0;
        var operationsList = relatedOperations.ToList();

        while (operationIndex < operationsList.Count && !workTimeTracker.IsComplete)
        {
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);

            if (ShouldProcessBreak(nextBreak, currentTime))
            {
                var elapsedWorkTimeBeforeBreak = workTimeTracker.ElapsedWorkTime;
                var elapsedWorkTimeForBreak = elapsedWorkTimeBeforeBreak;
                var breakResult = breakProcessor.ProcessBreak(
                    nextBreak!,
                    context.AuxiliaryOperations,
                    context.Indicators,
                    null,
                    ref order,
                    ref currentTime,
                    ref elapsedWorkTimeForBreak,
                    isFirst: false);

                var workTimeUsed = elapsedWorkTimeForBreak - elapsedWorkTimeBeforeBreak;
                if (workTimeUsed > TimeSpan.Zero)
                {
                    workTimeTracker.Add(workTimeUsed);
                }

                rows.AddRange(breakResult.Rows);
                breakIndex++;
                continue;
            }

            var operation = operationsList[operationIndex];
            var operationDuration = operation.Duration ?? TimeSpan.Zero;
            var adjustedDuration = workTimeTracker.GetAdjustedDuration(operationDuration);

            if (adjustedDuration <= TimeSpan.Zero)
                break;

            var operationEndTime = currentTime.Add(adjustedDuration);

            if (ShouldTrimOperationForBreak(nextBreak, operationEndTime))
            {
                operationEndTime = nextBreak!.StartTime;
                adjustedDuration = TimeHelper.CalculateDurationAcrossMidnight(currentTime, operationEndTime);
            }

            var operationRow = formRowDataFactory.CreateOperationTimeRow(
                order++,
                context.Indicators.OperationName,
                context.Indicators.PlanMinutes,
                context.Indicators.StartTimePlan,
                context.Indicators.EndTimePlan,
                currentTime,
                operationEndTime,
                operation,
                shiftStartMinutes);

            rows.Add(operationRow);
            currentTime = operationEndTime;
            workTimeTracker.Add(adjustedDuration);
            operationIndex++;
        }

        var remainingBreakRows = ProcessRemainingBreaks(
            context.SortedSchedules,
            breakIndex,
            order,
            context.AuxiliaryOperations,
            context.Indicators);

        rows.AddRange(remainingBreakRows);
        return rows;
    }

    private static bool ShouldProcessBreak(ShiftScheduleDto? nextBreak, TimeOnly currentTime)
    {
        if (nextBreak == null)
            return false;

        return currentTime >= nextBreak.StartTime;
    }

    private static bool ShouldTrimOperationForBreak(ShiftScheduleDto? nextBreak, TimeOnly operationEndTime)
    {
        if (nextBreak == null)
            return false;

        return operationEndTime > nextBreak.StartTime;
    }
}