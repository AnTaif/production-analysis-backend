using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class SingleProductInitializationStrategyBase(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager,
    ICleanupOperationHandler cleanupHandler
)
    : RowInitializationStrategyBase, IRowInitializationStrategy
{
    public override Task<ICollection<FormRowData>> InitializeAsync(RowInitializationContext context)
    {
        var productContext = context.FormContext.RequireContext<ProductContext>(FormContextAccessor.ProductContextKey);

        var (rows, endTime) = InitializeRowsForProduct(
            context.ShiftStartTime,
            context.SortedSchedules,
            context.Indicators,
            context.AuxiliaryOperations,
            productContext);

        var cleanupRow = cleanupHandler.CreateCleanupRow(
            endTime,
            GetNextOrder(rows),
            context.AuxiliaryOperations,
            context.Indicators);

        if (cleanupRow != null)
        {
            rows.Add(cleanupRow);
        }

        return Task.FromResult<ICollection<FormRowData>>(rows);
    }

    private (List<FormRowData> Rows, TimeOnly EndTime) InitializeRowsForProduct(
        TimeOnly shiftStartTime,
        IList<ShiftScheduleDto> sortedBreaks,
        InitializedIndicators indicators,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        ProductContext productContext)
    {
        var totalWorkTime = shiftTimeManager.GetTotalWorkTime();
        var rows = new List<FormRowData>();
        var currentTime = shiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        short order = 1;
        var hasWorkRows = false;

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;
            var workIntervalDuration = shiftTimeManager.CalculateWorkIntervalDuration(remainingWorkTime);
            var workIntervalEndTime = TimeHelper.AdjustForMidnight(currentTime, workIntervalDuration);

            if (ShouldProcessBreak(nextBreak, currentTime, workIntervalEndTime))
            {
                var isFirst = !hasWorkRows && currentTime >= nextBreak!.StartTime;

                var breakResult = breakProcessor.ProcessBreak(
                    nextBreak!,
                    auxiliaryOperations,
                    indicators,
                    productContext,
                    ref order,
                    ref currentTime,
                    ref elapsedWorkTime,
                    isFirst);

                rows.AddRange(breakResult.Rows);
                breakIndex++;
            }
            else
            {
                var workRow = formRowDataFactory.CreateWorkRow(
                    order++,
                    indicators.WorkTime!,
                    indicators.Plan,
                    currentTime,
                    workIntervalEndTime,
                    productContext);

                rows.Add(workRow);
                hasWorkRows = true;
                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workIntervalDuration);
            }
        }

        var remainingBreaks = cleanupHandler.FilterOutCleanup(
            sortedBreaks.Skip(breakIndex).ToList());

        if (remainingBreaks.Count > 0)
        {
            var remainingBreakRows = breakProcessor.ProcessRemainingBreaks(
                remainingBreaks,
                order,
                auxiliaryOperations,
                indicators,
                productContext,
                isLast: true);

            rows.AddRange(remainingBreakRows);

            if (remainingBreakRows.Count > 0)
            {
                var lastBreak = remainingBreaks.Last();
                if (auxiliaryOperations.TryGetValue(lastBreak.AuxiliaryOperationId, out var lastBreakOp))
                {
                    currentTime = lastBreak.StartTime.Add(lastBreakOp.Duration);
                }
            }
        }

        return (rows, currentTime);
    }

    private bool ShouldProcessBreak(
        ShiftScheduleDto? nextBreak,
        TimeOnly currentTime,
        TimeOnly workIntervalEndTime)
    {
        if (nextBreak == null)
            return false;

        if (cleanupHandler.IsCleanupOperation(nextBreak.AuxiliaryOperationId))
            return false;

        return breakProcessor.ShouldInsertBreak(currentTime, nextBreak, workIntervalEndTime);
    }

    private static short GetNextOrder(List<FormRowData> rows)
    {
        return (short)(rows.Count > 0 ? rows.Max(r => r.Order) + 1 : 1);
    }
}