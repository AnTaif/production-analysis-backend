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

        var (rows, endTime) = InitializeRowsForProduct(
            context.ShiftStartTime,
            context.SortedSchedules,
            context.Indicators,
            context.AuxiliaryOperations,
            productContext);

        // Добавляем уборку после продукта
        const int cleanupOperationId = 3; // ID операции "Уборка 15 мин"
        if (context.AuxiliaryOperations.TryGetValue(cleanupOperationId, out var cleanupOperation))
        {
            var cleanupStartTime = endTime;
            var cleanupEndTime = cleanupStartTime.Add(cleanupOperation.Duration);
            var cleanupOrder = (short)(rows.Count > 0 ? rows.Max(r => r.Order) + 1 : 1);

            var cleanupRow = formRowDataFactory.CreateBreakRow(
                cleanupOrder,
                context.Indicators.WorkTime,
                cleanupStartTime,
                cleanupEndTime,
                cleanupOperation.Name,
                cleanupOperationId,
                null); // Уборка не связана с продуктом

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
        var hasWorkRows = false; // Отслеживаем, были ли созданы рабочие строки
        const int cleanupOperationId = 3; // ID операции "Уборка 15 мин"

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;
            var workIntervalDuration = shiftTimeManager.CalculateWorkIntervalDuration(remainingWorkTime);
            var workIntervalEndTime = currentTime.Add(workIntervalDuration);

            // Пропускаем уборку из расписания - она добавляется после продукта
            if (nextBreak != null && nextBreak.AuxiliaryOperationId != cleanupOperationId &&
                breakProcessor.ShouldInsertBreak(currentTime, nextBreak, workIntervalEndTime))
            {
                // Определяем, является ли это первой операцией (нет рабочих строк и нет рабочего времени до перерыва)
                var isFirst = !hasWorkRows && currentTime >= nextBreak.StartTime;

                var breakResult = breakProcessor.ProcessBreak(
                    nextBreak,
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
            else if (nextBreak != null && nextBreak.AuxiliaryOperationId == cleanupOperationId)
            {
                // Пропускаем уборку из расписания - она будет добавлена после продукта
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
                hasWorkRows = true; // Отмечаем, что создана рабочая строка
                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workIntervalDuration);
            }
        }

        // Исключаем уборку из оставшихся перерывов - она добавляется после продукта
        var remainingBreaksWithoutCleanup = sortedBreaks
            .Skip(breakIndex)
            .Where(b => b.AuxiliaryOperationId != cleanupOperationId)
            .ToList();

        if (remainingBreaksWithoutCleanup.Count > 0)
        {
            var remainingBreakRows = ProcessRemainingBreaks(
                sortedBreaks,
                breakIndex,
                order,
                auxiliaryOperations,
                indicators,
                breakProcessor,
                productContext,
                isLast: true); // Все оставшиеся перерывы - последние

            // Фильтруем уборку из результата
            var filteredBreakRows = remainingBreakRows
                .Where(r => r.AuxiliaryOperationId != cleanupOperationId)
                .ToList();
            rows.AddRange(filteredBreakRows);

            // Обновляем currentTime на основе последнего перерыва
            if (filteredBreakRows.Count > 0)
            {
                var lastBreak = remainingBreaksWithoutCleanup.Last();
                if (auxiliaryOperations.TryGetValue(lastBreak.AuxiliaryOperationId, out var lastBreakOp))
                {
                    currentTime = lastBreak.StartTime.Add(lastBreakOp.Duration);
                }
            }
        }

        return (rows, currentTime);
    }
}