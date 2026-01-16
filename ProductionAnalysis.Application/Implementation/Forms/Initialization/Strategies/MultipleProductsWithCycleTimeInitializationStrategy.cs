using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

/// <summary>
///     Стратегия инициализации для нескольких продуктов с цикловым временем
/// </summary>
public class MultipleProductsWithCycleTimeInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager
)
    : RowInitializationStrategyBase, IRowInitializationStrategy
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.MultipleProductsWithCycleTime;
    }

    public override Task<ICollection<FormRowData>> InitializeAsync(RowInitializationContext context)
    {
        var multiProducts =
            context.FormContext.RequireContext<MultiProductContext>(FormContextAccessor.MultiProductContextKey);

        var allRows = new List<FormRowData>();
        short globalOrder = 1;

        foreach (var productContext in multiProducts.Products)
        {
            var productRows = InitializeRowsForProduct(
                context.ShiftStartTime,
                context.SortedSchedules,
                context.Indicators,
                context.AuxiliaryOperations,
                productContext,
                ref globalOrder);

            allRows.AddRange(productRows);
        }

        return Task.FromResult<ICollection<FormRowData>>(allRows);
    }

    private List<FormRowData> InitializeRowsForProduct(
        TimeOnly shiftStartTime,
        IList<ShiftScheduleDto> sortedBreaks,
        InitializedIndicators indicators,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        ProductContext productContext,
        ref short order)
    {
        var totalWorkTime = shiftTimeManager.GetTotalWorkTime();
        var rows = new List<FormRowData>();
        var currentTime = shiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        var localOrder = order;

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
                    ref localOrder,
                    ref currentTime,
                    ref elapsedWorkTime);

                rows.AddRange(breakResult.Rows);
                breakIndex++;
            }
            else
            {
                var workRow = formRowDataFactory.CreateWorkRow(
                    localOrder++,
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
            localOrder,
            auxiliaryOperations,
            indicators,
            breakProcessor,
            productContext);
        rows.AddRange(remainingBreakRows);

        order = (short)(localOrder + remainingBreakRows.Count);
        return rows;
    }
}