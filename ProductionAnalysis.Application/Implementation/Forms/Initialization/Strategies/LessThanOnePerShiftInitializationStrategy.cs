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
        const int cleanupOperationId = 3; // ID операции "Уборка 15 мин"

        while (operationIndex < operationsList.Count)
        {
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);

            // Проверяем, нужно ли вставить перерыв перед следующей операцией
            // Пропускаем уборку из расписания - она добавляется после всех операций

            if (nextBreak != null && nextBreak.AuxiliaryOperationId != cleanupOperationId &&
                currentTime >= nextBreak.StartTime)
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
            else if (nextBreak != null && nextBreak.AuxiliaryOperationId == cleanupOperationId)
            {
                // Пропускаем уборку из расписания - она будет добавлена после всех операций
                breakIndex++;
                continue;
            }

            // Создаем строку для текущей операции
            var operation = operationsList[operationIndex];
            var operationDuration = operation.Duration ?? TimeSpan.Zero;
            var operationEndTime = currentTime.Add(operationDuration);

            // Проверяем, не пересекается ли операция с перерывом (исключая уборку)
            if (nextBreak != null && nextBreak.AuxiliaryOperationId != cleanupOperationId &&
                operationEndTime > nextBreak.StartTime)
            {
                // Операция пересекается с перерывом, обрезаем её до начала перерыва
                operationEndTime = nextBreak.StartTime;
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

        // Исключаем уборку из оставшихся перерывов - она добавляется после всех операций
        var remainingBreaksWithoutCleanup = context.SortedSchedules
            .Skip(breakIndex)
            .Where(b => b.AuxiliaryOperationId != cleanupOperationId)
            .ToList();

        if (remainingBreaksWithoutCleanup.Count > 0)
        {
            var remainingBreakRows = ProcessRemainingBreaks(
                context.SortedSchedules,
                breakIndex,
                order,
                context.AuxiliaryOperations,
                context.Indicators,
                breakProcessor,
                null);

            // Фильтруем уборку из результата
            var filteredBreakRows = remainingBreakRows
                .Where(r => r.AuxiliaryOperationId != cleanupOperationId)
                .ToList();
            rows.AddRange(filteredBreakRows);

            // Обновляем currentTime на основе последнего перерыва
            if (filteredBreakRows.Count > 0)
            {
                var lastBreak = remainingBreaksWithoutCleanup.Last();
                if (context.AuxiliaryOperations.TryGetValue(lastBreak.AuxiliaryOperationId, out var lastBreakOp))
                {
                    currentTime = lastBreak.StartTime.Add(lastBreakOp.Duration);
                }
            }
        }

        // Добавляем уборку после всех операций
        if (context.AuxiliaryOperations.TryGetValue(cleanupOperationId, out var cleanupOperation))
        {
            var cleanupStartTime = currentTime;
            var cleanupEndTime = cleanupStartTime.Add(cleanupOperation.Duration);
            var cleanupOrder = (short)(rows.Count > 0 ? rows.Max(r => r.Order) + 1 : 1);

            var cleanupRow = formRowDataFactory.CreateBreakRow(
                cleanupOrder,
                context.Indicators.WorkTime,
                cleanupStartTime,
                cleanupEndTime,
                cleanupOperation.Name,
                cleanupOperationId,
                null);

            rows.Add(cleanupRow);
        }

        return rows;
    }
}