using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Constants;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

public class MultipleProductsWithCycleTimeInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager,
    IPlanCalculator planCalculator,
    ICleanupOperationHandler cleanupHandler,
    IRetoolingOperationHandler retoolingHandler
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
        var currentTime = context.ShiftStartTime;
        var globalBreakIndex = 0;

        var productsList = multiProducts.Products.ToList();
        for (var i = 0; i < productsList.Count; i++)
        {
            var productContext = productsList[i];
            var isLastProduct = i == productsList.Count - 1;

            var (productRows, endTime, newBreakIndex) = InitializeRowsForProduct(
                currentTime,
                context.SortedSchedules,
                context.Indicators,
                context.AuxiliaryOperations,
                productContext,
                globalBreakIndex,
                isLastProduct,
                ref globalOrder);

            allRows.AddRange(productRows);
            globalBreakIndex = newBreakIndex;

            if (!isLastProduct)
            {
                var retoolingRow = retoolingHandler.CreateRetoolingRow(
                    endTime,
                    globalOrder++,
                    context.AuxiliaryOperations,
                    context.Indicators);

                if (retoolingRow != null)
                {
                    allRows.Add(retoolingRow);
                    currentTime = endTime.Add(context.AuxiliaryOperations[AuxiliaryOperationIds.Retooling].Duration);
                }
                else
                {
                    currentTime = endTime;
                }
            }
            else
            {
                currentTime = endTime;
            }
        }

        var cleanupRow = cleanupHandler.CreateCleanupRow(
            currentTime,
            globalOrder++,
            context.AuxiliaryOperations,
            context.Indicators);

        if (cleanupRow != null)
        {
            allRows.Add(cleanupRow);
        }

        return Task.FromResult<ICollection<FormRowData>>(allRows);
    }

    private (List<FormRowData> Rows, TimeOnly EndTime, int NewBreakIndex) InitializeRowsForProduct(
        TimeOnly shiftStartTime,
        IList<ShiftScheduleDto> sortedBreaks,
        InitializedIndicators indicators,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        ProductContext productContext,
        int startBreakIndex,
        bool isLastProduct,
        ref short order)
    {
        var totalWorkTime = shiftTimeManager.GetTotalWorkTime();
        var rows = new List<FormRowData>();
        var currentTime = shiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = startBreakIndex;
        var localOrder = order;
        var hasWorkRows = false;
        var accumulatedPlan = 0;

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;
            var workIntervalDuration = shiftTimeManager.CalculateWorkIntervalDuration(remainingWorkTime);
            var preliminaryEndTime = TimeHelper.AdjustForMidnight(currentTime, workIntervalDuration);

            if (ShouldProcessBreak(nextBreak, currentTime, preliminaryEndTime))
            {
                var isFirst = !hasWorkRows && currentTime >= nextBreak!.StartTime;

                var breakResult = breakProcessor.ProcessBreak(
                    nextBreak,
                    auxiliaryOperations,
                    indicators,
                    productContext,
                    ref localOrder,
                    ref currentTime,
                    ref elapsedWorkTime,
                    isFirst);

                rows.AddRange(breakResult.Rows);
                breakIndex++;
            }
            else
            {
                if (accumulatedPlan >= productContext.DailyRate)
                    break;

                var workIntervalEndTime = CalculateWorkIntervalEndTime(
                    currentTime,
                    remainingWorkTime,
                    productContext,
                    accumulatedPlan);

                var intervalPlan = planCalculator.Calculate(currentTime, workIntervalEndTime, productContext);

                if (intervalPlan <= 0)
                    break;

                var workRow = formRowDataFactory.CreateWorkRow(
                    localOrder++,
                    indicators.WorkTime!,
                    indicators.Plan,
                    currentTime,
                    workIntervalEndTime,
                    productContext);

                rows.Add(workRow);
                hasWorkRows = true;
                accumulatedPlan += intervalPlan;

                var workDuration = TimeHelper.CalculateDurationAcrossMidnight(currentTime, workIntervalEndTime);
                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workDuration);

                if (accumulatedPlan >= productContext.DailyRate)
                    break;
            }
        }

        var remainingBreakRows = ProcessRemainingBreaksForProduct(
            sortedBreaks,
            breakIndex,
            localOrder,
            auxiliaryOperations,
            indicators,
            productContext,
            isLastProduct);

        rows.AddRange(remainingBreakRows);

        var endTime = CalculateEndTime(currentTime, remainingBreakRows, sortedBreaks, breakIndex, auxiliaryOperations,
            isLastProduct);

        order = (short)(localOrder + remainingBreakRows.Count);
        return (rows, endTime, breakIndex + remainingBreakRows.Count);
    }

    private bool ShouldProcessBreak(ShiftScheduleDto? nextBreak, TimeOnly currentTime, TimeOnly workIntervalEndTime)
    {
        if (nextBreak == null)
            return false;

        if (cleanupHandler.IsCleanupOperation(nextBreak.AuxiliaryOperationId))
            return false;

        return breakProcessor.ShouldInsertBreak(currentTime, nextBreak, workIntervalEndTime);
    }

    private TimeOnly CalculateWorkIntervalEndTime(
        TimeOnly currentTime,
        TimeSpan remainingWorkTime,
        ProductContext productContext,
        int accumulatedPlan)
    {
        var workIntervalDuration = shiftTimeManager.CalculateWorkIntervalDuration(remainingWorkTime);
        var workIntervalEndTime = TimeHelper.AdjustForMidnight(currentTime, workIntervalDuration);

        if (accumulatedPlan >= productContext.DailyRate)
            return currentTime;

        var intervalPlan = planCalculator.Calculate(currentTime, workIntervalEndTime, productContext);

        if (accumulatedPlan + intervalPlan > productContext.DailyRate)
        {
            workIntervalEndTime = CalculateLimitedEndTime(
                currentTime,
                remainingWorkTime,
                productContext,
                accumulatedPlan);
        }
        else
        {
            var maxEndTimeByWorkTime = TimeHelper.GetMaxEndTime(currentTime, remainingWorkTime);
            if (workIntervalEndTime > maxEndTimeByWorkTime)
            {
                workIntervalEndTime = maxEndTimeByWorkTime;
            }
        }

        return workIntervalEndTime;
    }

    private TimeOnly CalculateLimitedEndTime(
        TimeOnly currentTime,
        TimeSpan remainingWorkTime,
        ProductContext productContext,
        int accumulatedPlan)
    {
        var remainingPlan = productContext.DailyRate - accumulatedPlan;

        if (remainingPlan <= 0 || !productContext.CycleTime.HasValue || productContext.CycleTime.Value <= 0)
            return currentTime;

        var remainingSeconds = remainingPlan * productContext.CycleTime.Value;
        var remainingDuration = TimeSpan.FromSeconds(remainingSeconds);
        var limitedEndTime = TimeHelper.AdjustForMidnight(currentTime, remainingDuration);
        var maxEndTimeByWorkTime = TimeHelper.GetMaxEndTime(currentTime, remainingWorkTime);

        return limitedEndTime < maxEndTimeByWorkTime ? limitedEndTime : maxEndTimeByWorkTime;
    }

    private List<FormRowData> ProcessRemainingBreaksForProduct(
        IList<ShiftScheduleDto> sortedBreaks,
        int breakIndex,
        short localOrder,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext productContext,
        bool isLastProduct)
    {
        var remainingBreaks = cleanupHandler.FilterOutCleanup(
            sortedBreaks.Skip(breakIndex).ToList());

        if (remainingBreaks.Count == 0)
            return new List<FormRowData>();

        var remainingBreakRows = breakProcessor.ProcessRemainingBreaks(
            remainingBreaks,
            localOrder,
            auxiliaryOperations,
            indicators,
            productContext,
            isLast: isLastProduct).ToList();

        return remainingBreakRows;
    }

    private TimeOnly CalculateEndTime(
        TimeOnly currentTime,
        List<FormRowData> remainingBreakRows,
        IList<ShiftScheduleDto> sortedBreaks,
        int breakIndex,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        bool isLastProduct)
    {
        if (!isLastProduct || remainingBreakRows.Count == 0)
            return currentTime;

        var remainingBreaks = cleanupHandler.FilterOutCleanup(
            sortedBreaks.Skip(breakIndex).ToList());

        var maxEndTime = currentTime;

        foreach (var breakSchedule in remainingBreaks)
        {
            if (!auxiliaryOperations.TryGetValue(breakSchedule.AuxiliaryOperationId, out var breakOperation))
                continue;

            var breakEndTime = TimeHelper.AdjustForMidnight(
                breakSchedule.StartTime,
                breakOperation.Duration);

            if (breakEndTime > maxEndTime)
            {
                maxEndTime = breakEndTime;
            }
        }

        return maxEndTime;
    }
}