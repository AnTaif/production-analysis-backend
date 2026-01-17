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
    : OperationOrProductInitializationStrategyBase(operationService), IRowInitializationStrategy
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerShift;
    }

    public override async Task<ICollection<FormRowData>> InitializeAsync(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.RequireContext<OperationOrProductContext>(FormContextAccessor
                .OperationOrProductContextKey);
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

        var remainingBreaks = cleanupHandler.FilterOutCleanup(
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

            if (remainingBreakRows.Count > 0)
            {
                var lastBreak = remainingBreaks.Last();
                if (context.AuxiliaryOperations.TryGetValue(lastBreak.AuxiliaryOperationId, out var lastBreakOp))
                {
                    currentTime = lastBreak.StartTime.Add(lastBreakOp.Duration);
                }
            }
        }

        var cleanupRow = cleanupHandler.CreateCleanupRow(
            currentTime,
            GetNextOrder(rows),
            context.AuxiliaryOperations,
            context.Indicators);

        if (cleanupRow != null)
        {
            rows.Add(cleanupRow);
        }

        return rows;
    }

    private bool ShouldProcessBreak(ShiftScheduleDto? nextBreak, TimeOnly currentTime)
    {
        if (nextBreak == null)
            return false;

        if (cleanupHandler.IsCleanupOperation(nextBreak.AuxiliaryOperationId))
            return false;

        return currentTime >= nextBreak.StartTime;
    }

    private bool ShouldTrimOperationForBreak(ShiftScheduleDto? nextBreak, TimeOnly operationEndTime)
    {
        if (nextBreak == null)
            return false;

        if (cleanupHandler.IsCleanupOperation(nextBreak.AuxiliaryOperationId))
            return false;

        return operationEndTime > nextBreak.StartTime;
    }

    private static short GetNextOrder(List<FormRowData> rows)
    {
        return (short)(rows.Count > 0 ? rows.Max(r => r.Order) + 1 : 1);
    }
}