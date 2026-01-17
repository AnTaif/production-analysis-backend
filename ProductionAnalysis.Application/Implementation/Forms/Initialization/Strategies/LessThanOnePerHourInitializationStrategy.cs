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
            var timeUntilBreak =
                timeIntervalCalculator.CalculateTimeUntilBreak(currentTime, nextBreak, remainingWorkTime);

            if (ShouldProcessBreak(nextBreak, timeUntilBreak, cycleDuration))
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