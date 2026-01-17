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
    ICleanupOperationHandler cleanupHandler
)
    : OperationOrProductInitializationStrategyBase(operationService, cleanupHandler), IRowInitializationStrategy
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerShift;
    }

    protected override async Task<ICollection<FormRowData>> InitializeRowsAsync(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.Require<OperationOrProductContext>(FormContextAccessor.OperationOrProductContextKey);
        var relatedOperations = await GetRelatedOperationsAsync(operationContext);

        var shiftStartMinutes = context.ShiftStartTime.TotalMinutes();

        var rows = new List<FormRowData>();
        var currentTime = context.ShiftStartTime;
        var breakIndex = 0;
        short order = 1;
        var operationIndex = 0;
        var operationsList = relatedOperations.ToList();

        while (operationIndex < operationsList.Count)
        {
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);

            if (ShouldProcessBreak(nextBreak, currentTime))
            {
                var breakMetaInfo = context.AuxiliaryOperations[nextBreak!.AuxiliaryOperationId];
                var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

                var breakRow = formRowDataFactory.CreateBreakRow(
                    order++,
                    context.Indicators.WorkTime,
                    nextBreak.StartTime,
                    breakEndTime,
                    breakMetaInfo.Name,
                    nextBreak.AuxiliaryOperationId,
                    null);

                rows.Add(breakRow);
                currentTime = breakEndTime;
                breakIndex++;
                continue;
            }

            var operation = operationsList[operationIndex];
            var operationDuration = operation.Duration ?? TimeSpan.Zero;
            var operationEndTime = currentTime.Add(operationDuration);

            if (ShouldTrimOperationForBreak(nextBreak, operationEndTime))
            {
                operationEndTime = nextBreak!.StartTime;
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
            operationIndex++;
        }

        var remainingBreaks = FilterOutCleanup(
            context.SortedSchedules.Skip(breakIndex).ToList());

        if (remainingBreaks.Count > 0)
        {
            var remainingBreakRows = breakProcessor.ProcessRemainingBreaks(
                remainingBreaks,
                order,
                context.AuxiliaryOperations,
                context.Indicators,
                null);

            rows.AddRange(remainingBreakRows);
        }

        return rows;
    }

    private bool ShouldProcessBreak(ShiftScheduleDto? nextBreak, TimeOnly currentTime)
    {
        if (nextBreak == null)
            return false;

        return currentTime >= nextBreak.StartTime;
    }

    private bool ShouldTrimOperationForBreak(ShiftScheduleDto? nextBreak, TimeOnly operationEndTime)
    {
        if (nextBreak == null)
            return false;

        return operationEndTime > nextBreak.StartTime;
    }
}