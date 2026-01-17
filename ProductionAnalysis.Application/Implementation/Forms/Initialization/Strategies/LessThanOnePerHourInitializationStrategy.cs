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
    ICleanupOperationHandler cleanupHandler,
    ITimeIntervalCalculator timeIntervalCalculator,
    IFormRowEndTimeExtractor endTimeExtractor
)
    : OperationOrProductInitializationStrategyBase(operationService, cleanupHandler, endTimeExtractor, breakProcessor),
        IRowInitializationStrategy
{
    private readonly IBreakProcessor breakProcessor = breakProcessor;

    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerHour;
    }

    protected override ICollection<FormRowData> InitializeRows(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.Require<OperationOrProductContext>(FormContextAccessor.OperationOrProductContextKey);
        var relatedOperations = GetRelatedOperations(operationContext, context.AllOperations);

        var workTimeTracker = new WorkTimeTracker(shiftTimeManager);
        var cycleDuration = OperationService.CalculateCycleDuration(relatedOperations);

        var rows = new List<FormRowData>();
        var currentTime = context.ShiftStartTime;
        var breakIndex = 0;
        short order = 1;

        while (!workTimeTracker.IsComplete)
        {
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);
            var remainingWorkTime = workTimeTracker.RemainingWorkTime;
            var timeUntilBreak =
                timeIntervalCalculator.CalculateTimeUntilBreak(currentTime, nextBreak, remainingWorkTime);

            if (ShouldProcessBreak(nextBreak, timeUntilBreak, cycleDuration))
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
            }
            else if (remainingWorkTime.TotalSeconds >= cycleDuration)
            {
                var cycleDurationSpan = TimeSpan.FromSeconds(cycleDuration);
                var adjustedDuration = workTimeTracker.GetAdjustedDuration(cycleDurationSpan);

                if (adjustedDuration <= TimeSpan.Zero)
                    break;

                var cycleEndTime = currentTime.Add(adjustedDuration);

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
                workTimeTracker.Add(adjustedDuration);
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
                workTimeTracker.Add(remainingWorkTime);
                break;
            }
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

    private static bool ShouldProcessBreak(ShiftScheduleDto? nextBreak, double timeUntilBreak, double cycleDuration)
    {
        if (nextBreak == null)
            return false;

        return timeUntilBreak > 0 && timeUntilBreak < cycleDuration;
    }
}