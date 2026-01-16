using Core.Time;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

/// <summary>
///     Стратегия инициализации для операций (менее 1 шт. в смену)
/// </summary>
public class LessThanOnePerShiftInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IOperationService operationService
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

            // Проверяем, нужно ли вставить перерыв перед следующей операцией
            if (nextBreak != null && currentTime >= nextBreak.StartTime)
            {
                // Вставляем перерыв
                var breakMetaInfo = context.AuxiliaryOperations[nextBreak.AuxiliaryOperationId];
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

            // Создаем строку для текущей операции
            var operation = operationsList[operationIndex];
            var operationDuration = operation.Duration ?? TimeSpan.Zero;
            var operationEndTime = currentTime.Add(operationDuration);

            // Проверяем, не пересекается ли операция с перерывом
            if (nextBreak != null && operationEndTime > nextBreak.StartTime)
                // Операция пересекается с перерывом, обрезаем её до начала перерыва
                operationEndTime = nextBreak.StartTime;

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

        var remainingBreakRows = ProcessRemainingBreaks(
            context.SortedSchedules,
            breakIndex,
            order,
            context.AuxiliaryOperations,
            context.Indicators,
            breakProcessor,
            null);
        rows.AddRange(remainingBreakRows);

        return rows;
    }
}