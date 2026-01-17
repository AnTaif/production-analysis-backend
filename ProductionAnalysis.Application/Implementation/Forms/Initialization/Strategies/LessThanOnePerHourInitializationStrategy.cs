using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

/// <summary>
///     Стратегия инициализации для операций (менее 1 шт. в час)
/// </summary>
public class LessThanOnePerHourInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager,
    IOperationService operationService
)
    : OperationOrProductInitializationStrategyBase(operationService), IRowInitializationStrategy
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerHour;
    }

    public override async Task<ICollection<FormRowData>> InitializeAsync(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.RequireContext<OperationOrProductContext>(FormContextAccessor
                .OperationOrProductContextKey);
        var relatedOperations = await GetRelatedOperationsAsync(operationContext);

        var totalWorkTime = shiftTimeManager.GetTotalWorkTime();
        var cycleDuration = OperationService.CalculateCycleDuration(relatedOperations);

        var rows = new List<FormRowData>();
        var currentTime = context.ShiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        short order = 1;
        const int cleanupOperationId = 3;

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;

            // Проверяем, помещается ли полный цикл операций до следующего перерыва или конца смены
            var timeUntilBreak = nextBreak != null
                ? (nextBreak.StartTime - currentTime).TotalSeconds
                : remainingWorkTime.TotalSeconds;

            // Пропускаем уборку из расписания - она добавляется после всех операций
            // ID операции "Уборка 15 мин"
            if (nextBreak != null && nextBreak.AuxiliaryOperationId != cleanupOperationId &&
                timeUntilBreak > 0 && timeUntilBreak < cycleDuration)
            {
                // До перерыва не помещается полный цикл, обрабатываем перерыв
                // Для операций не добавляем elapsedWorkTime при обработке перерыва
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
                elapsedWorkTime = elapsedWorkTime.Add(breakMetaInfo.Duration);
                breakIndex++;
            }
            else if (nextBreak != null && nextBreak.AuxiliaryOperationId == cleanupOperationId)
            {
                // Пропускаем уборку из расписания - она будет добавлена после всех операций
                breakIndex++;
            }
            else if (remainingWorkTime.TotalSeconds >= cycleDuration)
            {
                // Помещается полный цикл операций
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
                // Осталось меньше времени, чем цикл, но больше 0 - создаем последний цикл
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