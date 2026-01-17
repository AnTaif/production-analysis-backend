using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class SingleProductInitializationStrategyBase(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    ICleanupOperationHandler cleanupHandler,
    IFormRowEndTimeExtractor endTimeExtractor
)
    : RowInitializationStrategyBase(cleanupHandler, endTimeExtractor, breakProcessor)
{
    private readonly IFormRowEndTimeExtractor endTimeExtractor = endTimeExtractor;
    private readonly IBreakProcessor breakProcessor = breakProcessor;

    protected override ICollection<FormRowData> InitializeRows(RowInitializationContext context)
    {
        var productContext = context.FormContext.Require<ProductContext>(FormContextAccessor.ProductContextKey);

        var (rows, _) = InitializeRowsForProduct(
            context.ShiftStartTime,
            context.SortedSchedules,
            context.Indicators,
            context.AuxiliaryOperations,
            productContext,
            context.WorkTimeTracker);

        return rows;
    }

    private (List<FormRowData> Rows, TimeOnly EndTime) InitializeRowsForProduct(
        TimeOnly shiftStartTime,
        IList<ShiftScheduleDto> sortedBreaks,
        InitializedIndicators indicators,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        ProductContext productContext,
        IWorkTimeTracker workTimeTracker)
    {
        var rows = new List<FormRowData>();
        var currentTime = shiftStartTime;
        var breakIndex = 0;
        short order = 1;
        var hasWorkRows = false;

        while (!workTimeTracker.IsComplete)
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var workIntervalDuration = workTimeTracker.GetNextWorkIntervalDuration();

            if (workIntervalDuration <= TimeSpan.Zero)
                break;

            var workIntervalEndTime = TimeHelper.AdjustForMidnight(currentTime, workIntervalDuration);

            if (ShouldProcessBreak(nextBreak, currentTime, workIntervalEndTime))
            {
                var isFirst = !hasWorkRows && currentTime >= nextBreak!.StartTime;

                var breakResult = ProcessBreakWithTracking(
                    nextBreak!,
                    auxiliaryOperations,
                    indicators,
                    productContext,
                    workTimeTracker,
                    ref order,
                    ref currentTime,
                    isFirst);

                rows.AddRange(breakResult.Rows);
                breakIndex++;
            }
            else
            {
                var actualDuration = workTimeTracker.AddAndGetActual(workIntervalDuration);
                var actualEndTime = TimeHelper.AdjustForMidnight(currentTime, actualDuration);

                var workRow = formRowDataFactory.CreateWorkRow(
                    order++,
                    indicators.WorkTime!,
                    indicators.Plan,
                    currentTime,
                    actualEndTime,
                    productContext);

                rows.Add(workRow);
                hasWorkRows = true;
                currentTime = actualEndTime;
            }
        }

        var remainingBreakRows = ProcessRemainingBreaks(
            sortedBreaks,
            breakIndex,
            order,
            auxiliaryOperations,
            indicators,
            productContext,
            isLast: true);

        rows.AddRange(remainingBreakRows);

        var endTime = remainingBreakRows.Count > 0
            ? endTimeExtractor.ExtractEndTime(remainingBreakRows.Last())
            : currentTime;

        return (rows, endTime);
    }

    private bool ShouldProcessBreak(
        ShiftScheduleDto? nextBreak,
        TimeOnly currentTime,
        TimeOnly workIntervalEndTime)
    {
        if (nextBreak == null)
            return false;

        return breakProcessor.ShouldInsertBreak(currentTime, nextBreak, workIntervalEndTime);
    }
}