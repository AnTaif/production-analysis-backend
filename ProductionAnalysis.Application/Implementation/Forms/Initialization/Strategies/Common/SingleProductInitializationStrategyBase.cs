using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class SingleProductInitializationStrategyBase(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager
)
    : RowInitializationStrategyBase, IRowInitializationStrategy
{
    public override Task<ICollection<FormRowData>> InitializeAsync(RowInitializationContext context)
    {
        var productContext = context.FormContext.RequireContext<ProductContext>(FormContextAccessor.ProductContextKey);

        var rows = InitializeRowsForProduct(
            context.ShiftStartTime,
            context.SortedSchedules,
            context.Indicators,
            context.AuxiliaryOperations,
            productContext);

        return Task.FromResult<ICollection<FormRowData>>(rows);
    }

    private List<FormRowData> InitializeRowsForProduct(
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

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;
            var workIntervalDuration = shiftTimeManager.CalculateWorkIntervalDuration(remainingWorkTime);
            var workIntervalEndTime = currentTime.Add(workIntervalDuration);

            if (nextBreak != null && breakProcessor.ShouldInsertBreak(currentTime, nextBreak, workIntervalEndTime))
            {
                var breakResult = breakProcessor.ProcessBreak(
                    nextBreak,
                    auxiliaryOperations,
                    indicators,
                    productContext,
                    ref order,
                    ref currentTime,
                    ref elapsedWorkTime);

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
                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workIntervalDuration);
            }
        }

        var remainingBreakRows = ProcessRemainingBreaks(
            sortedBreaks,
            breakIndex,
            order,
            auxiliaryOperations,
            indicators,
            breakProcessor);
        rows.AddRange(remainingBreakRows);

        return rows;
    }
}