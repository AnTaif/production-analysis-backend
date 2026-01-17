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
        const int cleanupOperationId = 3; // ID операции "Уборка 15 мин"
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

        // Добавляем уборку один раз в самом конце формы после всех продуктов
        if (context.AuxiliaryOperations.TryGetValue(cleanupOperationId, out var cleanupOperation))
        {
            var cleanupStartTime = currentTime;
            var cleanupEndTime = cleanupStartTime.Add(cleanupOperation.Duration);

            var cleanupRow = formRowDataFactory.CreateBreakRow(
                globalOrder++,
                context.Indicators.WorkTime,
                cleanupStartTime,
                cleanupEndTime,
                cleanupOperation.Name,
                cleanupOperationId,
                null); // Уборка не связана с продуктом

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
        var breakIndex = startBreakIndex; // Начинаем с глобального индекса
        var localOrder = order;
        var hasWorkRows = false; // Отслеживаем, были ли созданы рабочие строки
        var accumulatedPlan = 0; // Накопленный план для текущего продукта
        const int cleanupOperationId = 3; // ID операции "Уборка 15 мин"
        const int retoolingOperationId = 4; // ID операции "Переналадка 15 мин"

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;
            var workIntervalDuration = shiftTimeManager.CalculateWorkIntervalDuration(remainingWorkTime);
            var workIntervalEndTime = currentTime.Add(workIntervalDuration);

            // Проверяем, не переходит ли время через полночь
            // Если workIntervalEndTime < currentTime, значит перешли через полночь
            // В этом случае ограничиваем до конца дня
            if (workIntervalEndTime < currentTime)
            {
                var timeUntilMidnight = TimeSpan.FromDays(1) - TimeSpan.FromTicks(currentTime.Ticks);
                if (timeUntilMidnight < workIntervalDuration)
                {
                    workIntervalDuration = timeUntilMidnight;
                }

                // Вычисляем время до полуночи
                var minutesUntilMidnight = (int)timeUntilMidnight.TotalMinutes;
                workIntervalEndTime = currentTime.AddMinutes(minutesUntilMidnight);
                // Если все еще переходим через полночь, ограничиваем до 23:59
                if (workIntervalEndTime < currentTime)
                {
                    workIntervalEndTime = new TimeOnly(23, 59);
                }
            }

            // Пропускаем уборку из расписания - она добавляется в конце формы
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
                    ref localOrder,
                    ref currentTime,
                    ref elapsedWorkTime,
                    isFirst);

                rows.AddRange(breakResult.Rows);
                breakIndex++;
            }
            else if (nextBreak != null && nextBreak.AuxiliaryOperationId == cleanupOperationId)
            {
                // Пропускаем уборку из расписания - она будет добавлена в конце формы
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

                        // Проверяем переход через полночь
                        if (limitedEndTime < currentTime)
                        {
                            var timeUntilMidnight = TimeSpan.FromDays(1) - TimeSpan.FromTicks(currentTime.Ticks);
                            if (timeUntilMidnight < remainingDuration)
                            {
                                var minutesUntilMidnight = (int)timeUntilMidnight.TotalMinutes;
                                limitedEndTime = currentTime.AddMinutes(minutesUntilMidnight);
                                if (limitedEndTime < currentTime)
                                {
                                    limitedEndTime = new TimeOnly(23, 59);
                                }
                            }
                        }

                        // Ограничиваем также и по оставшемуся рабочему времени смены
                        // Вычисляем максимальное время окончания на основе оставшегося рабочего времени
                        var maxEndTimeByWorkTime = currentTime.Add(remainingWorkTime);

                        // Проверяем переход через полночь для maxEndTimeByWorkTime
                        if (maxEndTimeByWorkTime < currentTime)
                        {
                            var timeUntilMidnight = TimeSpan.FromDays(1) - TimeSpan.FromTicks(currentTime.Ticks);
                            var minutesUntilMidnight = (int)timeUntilMidnight.TotalMinutes;
                            maxEndTimeByWorkTime = currentTime.AddMinutes(minutesUntilMidnight);
                            if (maxEndTimeByWorkTime < currentTime)
                            {
                                maxEndTimeByWorkTime = new TimeOnly(23, 59);
                            }
                        }

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

                    // Проверяем переход через полночь
                    if (maxEndTimeByWorkTime < currentTime)
                    {
                        var timeUntilMidnight = TimeSpan.FromDays(1) - TimeSpan.FromTicks(currentTime.Ticks);
                        var minutesUntilMidnight = (int)timeUntilMidnight.TotalMinutes;
                        maxEndTimeByWorkTime = currentTime.AddMinutes(minutesUntilMidnight);
                        if (maxEndTimeByWorkTime < currentTime)
                        {
                            maxEndTimeByWorkTime = new TimeOnly(23, 59);
                        }
                    }

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
                // Если workDuration отрицательный, значит перешли через полночь
                if (workDuration < TimeSpan.Zero)
                {
                    workDuration = TimeSpan.FromDays(1) - TimeSpan.FromTicks(currentTime.Ticks) +
                                   TimeSpan.FromTicks(workIntervalEndTime.Ticks);
                }

                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workDuration);

                // Если достигли или превысили dailyRate, прекращаем создание строк
                if (accumulatedPlan >= productContext.DailyRate)
                {
                    break;
                }
            }
        }

        // Обрабатываем оставшиеся перерывы
        // Перерывы должны быть распределены по времени, а не все в конце
        // Для последнего продукта добавляем все оставшиеся перерывы
        // Для не последнего продукта проверяем, есть ли перерывы, которые должны быть перед переналадкой
        var remainingBreakRows = new List<FormRowData>();
        var remainingBreaks = sortedBreaks.Skip(breakIndex).ToList();

        if (remainingBreaks.Count > 0)
        {
            if (isLastProduct)
            {
                // Для последнего продукта добавляем все оставшиеся перерывы, отсортированные по времени
                // Исключаем уборку из расписания - она добавляется в конце формы
                var sortedRemainingBreaks = remainingBreaks
                    .Where(b => b.AuxiliaryOperationId != cleanupOperationId)
                    .OrderBy(b => b.StartTime)
                    .ToList();
                remainingBreakRows = breakProcessor.ProcessRemainingBreaks(
                    sortedRemainingBreaks,
                    localOrder,
                    auxiliaryOperations,
                    indicators,
                    productContext,
                    isLast: true).ToList();
                rows.AddRange(remainingBreakRows);
            }
            else
            {
                // Для не последнего продукта обрабатываем оставшиеся перерывы (кроме уборки, которая добавляется в конце формы)
                // Фильтруем уборку из оставшихся перерывов
                var breaksToProcess = remainingBreaks
                    .Where(b => b.AuxiliaryOperationId != cleanupOperationId)
                    .OrderBy(b => b.StartTime)
                    .ToList();

                if (breaksToProcess.Count > 0)
                {
                    var breaksToAdd = breakProcessor.ProcessRemainingBreaks(
                        breaksToProcess,
                        localOrder,
                        auxiliaryOperations,
                        indicators,
                        productContext,
                        isLast: false).ToList();
                    rows.AddRange(breaksToAdd);
                    localOrder += (short)breaksToAdd.Count;
                    breakIndex += breaksToProcess.Count;
                }
            }
        }

        // Вычисляем время окончания: берем максимальное время из оставшихся перерывов или текущее время
        var endTime = currentTime;
        if (isLastProduct && remainingBreakRows.Count > 0)
        {
            // Находим максимальное время окончания из оставшихся перерывов (исключая уборку)
            foreach (var breakSchedule in remainingBreaks.Where(b => b.AuxiliaryOperationId != cleanupOperationId))
            {
                if (auxiliaryOperations.TryGetValue(breakSchedule.AuxiliaryOperationId, out var breakOperation))
                {
                    var breakEndTime = breakSchedule.StartTime.Add(breakOperation.Duration);
                    // Проверяем переход через полночь
                    if (breakEndTime < breakSchedule.StartTime)
                    {
                        var timeUntilMidnight =
                            TimeSpan.FromDays(1) - TimeSpan.FromTicks(breakSchedule.StartTime.Ticks);
                        var minutesUntilMidnight = (int)timeUntilMidnight.TotalMinutes;
                        breakEndTime = breakSchedule.StartTime.AddMinutes(minutesUntilMidnight);
                        if (breakEndTime < breakSchedule.StartTime)
                        {
                            breakEndTime = new TimeOnly(23, 59);
                        }
                    }

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