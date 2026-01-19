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
    IOperationService operationService,
    ICleanupOperationHandler cleanupHandler,
    IFormRowEndTimeExtractor endTimeExtractor
)
    : OperationOrProductInitializationStrategyBase(operationService, cleanupHandler, endTimeExtractor, breakProcessor),
        IRowInitializationStrategy
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerShift;
    }

    protected override ICollection<FormRowData> InitializeRows(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.Require<OperationOrProductContext>(FormContextAccessor.OperationOrProductContextKey);
        var allRelatedOperations = GetRelatedOperations(operationContext, context.AllOperations);

        // Для типа 5 ПА используем только под-операции, исключая саму операцию из контекста
        var relatedOperations = operationContext is { IsOperationBased: true, OperationId: not null }
            ? allRelatedOperations.Where(op => op.Id != operationContext.OperationId.Value).ToList()
            : allRelatedOperations;

        var worktimeTracker = context.WorkTimeTracker;

        var shiftStartMinutes = context.ShiftStartTime.TotalMinutes();

        var rows = new List<FormRowData>();
        var breakIndex = 0;
        short order = 1;
        var operationIndex = 0;
        var operationsList = relatedOperations.ToList();

        while (operationIndex < operationsList.Count && !worktimeTracker.IsComplete)
        {
            var currentTime = worktimeTracker.CurrentTime;
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);

            if (ShouldProcessBreak(nextBreak, currentTime))
            {
                var breakResult = ProcessBreakWithTracking(
                    nextBreak!,
                    context.AuxiliaryOperations,
                    context.Indicators,
                    null,
                    worktimeTracker,
                    ref order,
                    isFirst: false);

                rows.AddRange(breakResult.Rows);
                breakIndex++;
                continue;
            }

            var operation = operationsList[operationIndex];
            var operationDuration = operation.Duration ?? TimeSpan.Zero;
            var operationEndTime = currentTime.Add(operationDuration);

            if (ShouldTrimOperationForBreak(nextBreak, operationEndTime))
            {
                operationEndTime = nextBreak!.StartTime;
                operationDuration = TimeHelper.CalculateDurationAcrossMidnight(currentTime, operationEndTime);
            }

            var actualDuration = worktimeTracker.AdvanceWorktime(operationDuration);

            if (actualDuration <= TimeSpan.Zero)
                break;

            if (actualDuration < operationDuration)
            {
                operationEndTime = TimeHelper.AdjustForMidnight(currentTime, actualDuration);
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
            operationIndex++;
        }

        var remainingBreakRows = ProcessRemainingBreaks(
            context.SortedSchedules,
            breakIndex,
            order,
            context.AuxiliaryOperations,
            context.Indicators,
            worktimeTracker);

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