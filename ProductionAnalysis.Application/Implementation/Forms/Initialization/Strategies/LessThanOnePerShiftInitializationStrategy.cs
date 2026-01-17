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

        var shiftStartMinutes = context.ShiftStartTime.TotalMinutes();
        var totalWorkTime = shiftTimeManager.GetTotalWorkTime();

        var rows = new List<FormRowData>();
        var currentTime = context.ShiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        short order = 1;
        var operationIndex = 0;
        var operationsList = relatedOperations.ToList();

        while (operationIndex < operationsList.Count &&
               !shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);

            if (ShouldProcessBreak(nextBreak, currentTime))
            {
                var breakResult = breakProcessor.ProcessBreak(
                    nextBreak!,
                    context.AuxiliaryOperations,
                    context.Indicators,
                    null,
                    ref order,
                    ref currentTime,
                    ref elapsedWorkTime,
                    isFirst: false);

                rows.AddRange(breakResult.Rows);
                breakIndex++;
                continue;
            }

            var operation = operationsList[operationIndex];
            var operationDuration = operation.Duration ?? TimeSpan.Zero;

            if (!shiftTimeManager.CanAddWorkInterval(elapsedWorkTime, operationDuration))
            {
                var remainingWorkTime = shiftTimeManager.GetRemainingWorkTime(elapsedWorkTime);
                if (remainingWorkTime <= TimeSpan.Zero)
                    break;

                operationDuration = remainingWorkTime;
            }

            var operationEndTime = currentTime.Add(operationDuration);

            if (ShouldTrimOperationForBreak(nextBreak, operationEndTime))
            {
                operationEndTime = nextBreak!.StartTime;
                operationDuration = TimeHelper.CalculateDurationAcrossMidnight(currentTime, operationEndTime);
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
            elapsedWorkTime = elapsedWorkTime.Add(operationDuration);
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