using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

public class LessThanOnePerHourInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IOperationService operationService,
    ICleanupOperationHandler cleanupHandler,
    ITimeIntervalCalculator timeIntervalCalculator,
    IFormRowEndTimeExtractor endTimeExtractor
)
    : OperationOrProductInitializationStrategyBase(operationService, cleanupHandler, endTimeExtractor, breakProcessor),
        IRowInitializationStrategy
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerHour;
    }

    protected override ICollection<FormRowData> InitializeRows(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.Require<OperationOrProductContext>(FormContextAccessor.OperationOrProductContextKey);
        var relatedOperations = GetRelatedOperations(operationContext, context.AllOperations);
        var worktimeTracker = context.WorkTimeTracker;

        var cycleDuration = OperationService.CalculateCycleDuration(relatedOperations);

        var rows = new List<FormRowData>();
        var breakIndex = 0;
        short order = 1;

        while (!worktimeTracker.IsComplete)
        {
            var currentTime = worktimeTracker.CurrentTime;
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);
            var remainingWorkTime = worktimeTracker.RemainingWorkTime;
            var timeUntilBreak =
                timeIntervalCalculator.CalculateTimeUntilBreak(currentTime, nextBreak, remainingWorkTime);

            if (ShouldProcessBreak(nextBreak, timeUntilBreak, cycleDuration))
            {
                var breakResult = ProcessBreakWithTracking(
                    nextBreak!,
                    context.AuxiliaryOperations,
                    context.Indicators,
                    null,
                    worktimeTracker,
                    ref order,
                    ref currentTime,
                    isFirst: false);

                rows.AddRange(breakResult.Rows);
                breakIndex++;
            }
            else if (remainingWorkTime.TotalSeconds >= cycleDuration)
            {
                var cycleDurationSpan = TimeSpan.FromSeconds(cycleDuration);
                var actualDuration = worktimeTracker.AdvanceWorktime(cycleDurationSpan);

                if (actualDuration <= TimeSpan.Zero)
                    break;

                var cycleEndTime = currentTime.Add(actualDuration);

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
            }
            else
            {
                var actualDuration = worktimeTracker.AdvanceWorktime(remainingWorkTime);
                var cycleEndTime = currentTime.Add(actualDuration);

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

    private static bool ShouldProcessBreak(ShiftScheduleDto? nextBreak, double timeUntilBreak, double cycleDuration)
    {
        if (nextBreak == null)
            return false;

        return timeUntilBreak > 0 && timeUntilBreak < cycleDuration;
    }
}