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
    IShiftTimeManager shiftTimeManager,
    IPlanCalculator planCalculator
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
        var globalBreakIndex = 0; // Глобальный индекс перерывов для всех продуктов
        const int retoolingOperationId = 4; // ID операции "Переналадка 15 мин"

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
            globalBreakIndex = newBreakIndex; // Обновляем глобальный индекс перерывов

            // Добавляем операцию "Переналадка 15 мин" между продуктами (но не после последнего)
            if (!isLastProduct &&
                context.AuxiliaryOperations.TryGetValue(retoolingOperationId, out var retoolingOperation))
            {
                var retoolingStartTime = endTime;
                var retoolingEndTime = retoolingStartTime.Add(retoolingOperation.Duration);

                var retoolingRow = formRowDataFactory.CreateBreakRow(
                    globalOrder++,
                    context.Indicators.WorkTime,
                    retoolingStartTime,
                    retoolingEndTime,
                    retoolingOperation.Name,
                    retoolingOperationId,
                    null); // Переналадка не связана с продуктом

                allRows.Add(retoolingRow);
                currentTime = retoolingEndTime;
            }
            else
            {
                currentTime = endTime;
            }
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
        var breakIndex = startBreakIndex; // Начинаем с глобального индекса
        var localOrder = order;
        var hasWorkRows = false; // Отслеживаем, были ли созданы рабочие строки
        var accumulatedPlan = 0; // Накопленный план для текущего продукта

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;
            var workIntervalDuration = shiftTimeManager.CalculateWorkIntervalDuration(remainingWorkTime);
            var workIntervalEndTime = currentTime.Add(workIntervalDuration);

            if (nextBreak != null && breakProcessor.ShouldInsertBreak(currentTime, nextBreak, workIntervalEndTime))
            {
                // Определяем, является ли это первой операцией (нет рабочих строк и нет рабочего времени до перерыва)
                var isFirst = !hasWorkRows && currentTime >= nextBreak.StartTime;

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
                // Проверяем, не превысили ли мы dailyRate
                if (accumulatedPlan >= productContext.DailyRate)
                {
                    break; // Прекращаем создание строк, если достигли dailyRate
                }

                // Вычисляем план для текущего интервала
                var intervalPlan = planCalculator.Calculate(currentTime, workIntervalEndTime, productContext);

                // Если добавление этого интервала превысит dailyRate, ограничиваем его
                if (accumulatedPlan + intervalPlan > productContext.DailyRate)
                {
                    // Вычисляем, сколько времени нужно для оставшегося плана
                    var remainingPlan = productContext.DailyRate - accumulatedPlan;
                    if (remainingPlan > 0 && productContext.CycleTime.HasValue && productContext.CycleTime.Value > 0)
                    {
                        var remainingSeconds = remainingPlan * productContext.CycleTime.Value;
                        var remainingDuration = TimeSpan.FromSeconds(remainingSeconds);
                        var limitedEndTime = currentTime.Add(remainingDuration);

                        // Ограничиваем также и по оставшемуся рабочему времени смены
                        // Вычисляем максимальное время окончания на основе оставшегося рабочего времени
                        var maxEndTimeByWorkTime = currentTime.Add(remainingWorkTime);

                        // Берем минимальное из двух ограничений
                        workIntervalEndTime = limitedEndTime < maxEndTimeByWorkTime
                            ? limitedEndTime
                            : maxEndTimeByWorkTime;

                        // Пересчитываем план для ограниченного интервала
                        intervalPlan = planCalculator.Calculate(currentTime, workIntervalEndTime, productContext);
                        // Убеждаемся, что не превышаем dailyRate из-за округления
                        if (accumulatedPlan + intervalPlan > productContext.DailyRate)
                        {
                            intervalPlan = remainingPlan; // Используем точное значение оставшегося плана
                        }
                    }
                    else
                    {
                        break; // Если не можем вычислить оставшееся время, прекращаем
                    }
                }
                else
                {
                    // Даже если не превышаем dailyRate, нужно убедиться, что не превышаем оставшееся рабочее время
                    var maxEndTimeByWorkTime = currentTime.Add(remainingWorkTime);
                    if (workIntervalEndTime > maxEndTimeByWorkTime)
                    {
                        workIntervalEndTime = maxEndTimeByWorkTime;
                        // Пересчитываем план для ограниченного интервала
                        intervalPlan = planCalculator.Calculate(currentTime, workIntervalEndTime, productContext);
                    }
                }

                // Если план стал 0 или отрицательным, прекращаем
                if (intervalPlan <= 0)
                {
                    break;
                }

                var workRow = formRowDataFactory.CreateWorkRow(
                    localOrder++,
                    indicators.WorkTime!,
                    indicators.Plan,
                    currentTime,
                    workIntervalEndTime,
                    productContext);

                rows.Add(workRow);
                hasWorkRows = true; // Отмечаем, что создана рабочая строка
                accumulatedPlan += intervalPlan; // Обновляем накопленный план

                var workDuration = workIntervalEndTime - currentTime;
                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workDuration);

                // Если достигли или превысили dailyRate, прекращаем создание строк
                if (accumulatedPlan >= productContext.DailyRate)
                {
                    break;
                }
            }
        }

        // Обрабатываем оставшиеся перерывы только для последнего продукта
        var remainingBreakRows = new List<FormRowData>();
        if (isLastProduct)
        {
            remainingBreakRows = ProcessRemainingBreaks(
                sortedBreaks,
                breakIndex,
                localOrder,
                auxiliaryOperations,
                indicators,
                breakProcessor,
                productContext,
                isLast: true).ToList(); // Все оставшиеся перерывы - последние
            rows.AddRange(remainingBreakRows);
        }

        // Вычисляем время окончания: берем максимальное время из оставшихся перерывов или текущее время
        var endTime = currentTime;
        if (isLastProduct && remainingBreakRows.Count > 0)
        {
            // Находим максимальное время окончания из оставшихся перерывов
            var remainingBreaks = sortedBreaks.Skip(breakIndex).ToList();
            foreach (var breakSchedule in remainingBreaks)
            {
                if (auxiliaryOperations.TryGetValue(breakSchedule.AuxiliaryOperationId, out var breakOperation))
                {
                    var breakEndTime = breakSchedule.StartTime.Add(breakOperation.Duration);
                    if (breakEndTime > endTime)
                    {
                        endTime = breakEndTime;
                    }
                }
            }
        }

        order = (short)(localOrder + remainingBreakRows.Count);
        return (rows, endTime, breakIndex);
    }
}