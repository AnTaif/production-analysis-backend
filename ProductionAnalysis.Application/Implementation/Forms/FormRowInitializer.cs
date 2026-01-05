using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Implementation.Forms.Context;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowInitializer
{
    Task<ICollection<FormRowData>> InitializeRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template,
        Dictionary<string, FormContext>? formContext = null);
}

[RegisterScoped]
public class FormRowInitializer(
    IPaUnitOfWork unitOfWork,
    IMultiProductContextExtractor multiProductContextExtractor,
    IFormRowDataFactory formRowDataFactory,
    ICumulativeValueCalculator cumulativeValueCalculator
) : IFormRowInitializer
{
    public async Task<ICollection<FormRowData>> InitializeRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template,
        Dictionary<string, FormContext>? formContext = null)
    {
        var indicators = ExtractIndicators(template);
        var auxiliaryOperationsByIds = await LoadAuxiliaryOperationsAsync();
        var sortedBreaks = schedules.OrderBy(s => s.StartTime).ToList();

        // Проверяем, есть ли контекст операций
        var operationContext = formContext?.GetOperationContext();
        if (operationContext != null)
        {
            return await InitializeRowsForOperationsAsync(
                shiftStartTime,
                sortedBreaks,
                template,
                indicators,
                auxiliaryOperationsByIds,
                operationContext);
        }

        // Проверяем, есть ли несколько продуктов
        var multiProducts = multiProductContextExtractor.Extract(formContext);
        if (multiProducts.Count > 1)
        {
            return await InitializeRowsForMultipleProductsAsync(
                shiftStartTime,
                sortedBreaks,
                template,
                indicators,
                auxiliaryOperationsByIds,
                formContext);
        }

        // Обратная совместимость: один продукт
        short order = 1;

        var productContext = formContext?.GetProductContext();
        if (productContext == null)
        {
            throw new InvalidOperationException("ProductContext is required for single product form initialization");
        }

        var rows = InitializeRowsForSingleProduct(
            shiftStartTime,
            sortedBreaks,
            indicators,
            auxiliaryOperationsByIds,
            productContext,
            ref order);

        cumulativeValueCalculator.FillCumulativeValues(rows, template.Indicators);

        return rows;
    }

    private async Task<ICollection<FormRowData>> InitializeRowsForMultipleProductsAsync(
        TimeOnly shiftStartTime,
        List<ShiftScheduleDto> sortedBreaks,
        Template template,
        InitializedIndicators indicators,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperationsByIds,
        Dictionary<string, FormContext>? formContext)
    {
        var multiProducts = multiProductContextExtractor.Extract(formContext);
        var allRows = new List<FormRowData>();
        short globalOrder = 1;

        foreach (var productContext in multiProducts)
        {
            var productRows = InitializeRowsForSingleProduct(
                shiftStartTime,
                sortedBreaks,
                indicators,
                auxiliaryOperationsByIds,
                productContext,
                ref globalOrder);

            allRows.AddRange(productRows);
        }

        cumulativeValueCalculator.FillCumulativeValues(allRows, template.Indicators);

        return allRows;
    }

    private ICollection<FormRowData> InitializeRowsForSingleProduct(
        TimeOnly shiftStartTime,
        List<ShiftScheduleDto> sortedBreaks,
        InitializedIndicators indicators,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperationsByIds,
        ProductContext? productContext,
        ref short order)
    {
        var totalWorkTime = TimeSpan.FromHours(ShiftConstants.ShiftDurationHours);

        var rows = new List<FormRowData>();
        var currentTime = shiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        var localOrder = order;

        while (elapsedWorkTime < totalWorkTime)
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;

            var workIntervalDuration = remainingWorkTime >= TimeSpan.FromHours(1)
                ? TimeSpan.FromHours(1)
                : remainingWorkTime;

            var workIntervalEndTime = currentTime.Add(workIntervalDuration);

            if (nextBreak != null && IsBreakInWorkInterval(nextBreak, currentTime, workIntervalEndTime))
            {
                localOrder = ProcessBreakInterval(
                    rows,
                    localOrder,
                    nextBreak,
                    auxiliaryOperationsByIds,
                    indicators,
                    productContext,
                    ref breakIndex,
                    ref currentTime,
                    ref elapsedWorkTime);
            }
            else
            {
                var workRow = formRowDataFactory.CreateWorkRow(
                    localOrder++,
                    indicators.WorkTime,
                    indicators.Plan,
                    currentTime,
                    workIntervalEndTime,
                    productContext);

                rows.Add(workRow);
                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workIntervalDuration);
            }
        }

        // Если чистое рабочее время закончилось, но остались перерывы - в цикле добавляем их в расписание
        while (breakIndex < sortedBreaks.Count)
        {
            var nextBreak = sortedBreaks[breakIndex];
            var breakMetaInfo = auxiliaryOperationsByIds[nextBreak.AuxiliaryOperationId];
            var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

            var breakRow = formRowDataFactory.CreateBreakRow(
                localOrder++,
                indicators.WorkTime,
                nextBreak.StartTime,
                breakEndTime,
                breakMetaInfo.Name,
                nextBreak.AuxiliaryOperationId);

            rows.Add(breakRow);
            breakIndex++;
        }

        order = localOrder;
        return rows;
    }

    private async Task<Dictionary<int, AuxiliaryOperationDto>> LoadAuxiliaryOperationsAsync()
    {
        var operations = await unitOfWork.Dictionaries.SelectAuxiliaryOperationsAsync();
        return operations.ToDictionary(ao => ao.Id);
    }

    private static InitializedIndicators ExtractIndicators(Template template)
    {
        return new InitializedIndicators
        {
            WorkTime = template.Indicators.Single(i => i.Id == ShiftConstants.WorktimeIndicatorId),
            Plan = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.PlanIndicatorId),
            OperationName = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.OperationNameIndicatorId),
            OperationTime = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.OperationTimeIndicatorId)
        };
    }

    private static ShiftScheduleDto? GetNextBreak(List<ShiftScheduleDto> sortedBreaks, int breakIndex)
    {
        return breakIndex < sortedBreaks.Count ? sortedBreaks[breakIndex] : null;
    }

    private static bool IsBreakInWorkInterval(ShiftScheduleDto breakSchedule, TimeOnly intervalStart,
        TimeOnly intervalEnd)
    {
        return intervalStart <= breakSchedule.StartTime && breakSchedule.StartTime < intervalEnd;
    }

    private short ProcessBreakInterval(
        List<FormRowData> rows,
        short order,
        ShiftScheduleDto nextBreak,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperationsByIds,
        InitializedIndicators indicators,
        ProductContext? productContext,
        ref int breakIndex,
        ref TimeOnly currentTime,
        ref TimeSpan elapsedWorkTime)
    {
        if (currentTime < nextBreak.StartTime)
        {
            var workDuration = nextBreak.StartTime - currentTime;
            var workRowBeforeBreak = formRowDataFactory.CreateWorkRow(
                order++,
                indicators.WorkTime,
                indicators.Plan,
                currentTime,
                nextBreak.StartTime,
                productContext);

            rows.Add(workRowBeforeBreak);

            elapsedWorkTime = elapsedWorkTime.Add(workDuration);
        }

        var breakMetaInfo = auxiliaryOperationsByIds[nextBreak.AuxiliaryOperationId];
        var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

        var breakRow = formRowDataFactory.CreateBreakRow(
            order++,
            indicators.WorkTime,
            nextBreak.StartTime,
            breakEndTime,
            breakMetaInfo.Name,
            nextBreak.AuxiliaryOperationId);

        rows.Add(breakRow);

        currentTime = breakEndTime;
        breakIndex++;

        return order;
    }

    private async Task<ICollection<FormRowData>> InitializeRowsForOperationsAsync(
        TimeOnly shiftStartTime,
        List<ShiftScheduleDto> sortedBreaks,
        Template template,
        InitializedIndicators indicators,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperationsByIds,
        OperationContext operationContext)
    {
        var allOperations = await LoadOperationsAsync();
        var relatedOperations = GetRelatedOperations(operationContext.OperationId, allOperations);

        if (relatedOperations.Count == 0)
        {
            throw new InvalidOperationException($"No operations found for operation id {operationContext.OperationId}");
        }

        var totalWorkTime = TimeSpan.FromHours(ShiftConstants.ShiftDurationHours);
        var cycleDuration = CalculateCycleDuration(relatedOperations);

        var rows = new List<FormRowData>();
        var currentTime = shiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        short order = 1;

        while (elapsedWorkTime < totalWorkTime)
        {
            var nextBreak = GetNextBreak(sortedBreaks, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;

            // Проверяем, помещается ли полный цикл операций до следующего перерыва или конца смены
            var timeUntilBreak = nextBreak != null
                ? (nextBreak.StartTime - currentTime).TotalSeconds
                : remainingWorkTime.TotalSeconds;

            if (nextBreak != null && timeUntilBreak > 0 && timeUntilBreak < cycleDuration)
            {
                // До перерыва не помещается полный цикл, обрабатываем перерыв
                order = ProcessBreakIntervalForOperations(
                    rows,
                    order,
                    nextBreak,
                    auxiliaryOperationsByIds,
                    indicators,
                    ref breakIndex,
                    ref currentTime,
                    ref elapsedWorkTime);
            }
            else if (remainingWorkTime.TotalSeconds >= cycleDuration)
            {
                // Помещается полный цикл операций
                var cycleEndTime = currentTime.Add(TimeSpan.FromSeconds(cycleDuration));

                var cycleRows = formRowDataFactory.CreateOperationCycleRows(
                    ref order,
                    indicators.WorkTime,
                    indicators.Plan,
                    indicators.OperationName,
                    indicators.OperationTime,
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
                    indicators.WorkTime,
                    indicators.Plan,
                    indicators.OperationName,
                    indicators.OperationTime,
                    currentTime,
                    cycleEndTime,
                    relatedOperations);

                rows.AddRange(cycleRows);
                elapsedWorkTime = totalWorkTime;
                break;
            }
        }

        // Если чистое рабочее время закончилось, но остались перерывы - добавляем их
        while (breakIndex < sortedBreaks.Count)
        {
            var nextBreak = sortedBreaks[breakIndex];
            var breakMetaInfo = auxiliaryOperationsByIds[nextBreak.AuxiliaryOperationId];
            var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

            var breakRow = formRowDataFactory.CreateBreakRow(
                order++,
                indicators.WorkTime,
                nextBreak.StartTime,
                breakEndTime,
                breakMetaInfo.Name,
                nextBreak.AuxiliaryOperationId);

            rows.Add(breakRow);
            breakIndex++;
        }

        cumulativeValueCalculator.FillCumulativeValues(rows, template.Indicators);

        return rows;
    }

    private async Task<ICollection<OperationDto>> LoadOperationsAsync()
    {
        return await unitOfWork.Dictionaries.SelectOperationsAsync();
    }

    private static ICollection<OperationDto> GetRelatedOperations(int operationId,
        ICollection<OperationDto> allOperations)
    {
        var operationsById = allOperations.ToDictionary(op => op.Id);
        var result = new List<OperationDto>();
        var visited = new HashSet<int>();

        if (!operationsById.TryGetValue(operationId, out var mainOperation))
        {
            return result;
        }

        // Добавляем основную операцию
        result.Add(mainOperation);
        visited.Add(operationId);

        // Собираем все операции, которые связаны с основной операцией или продуктом
        // через BasedOperationId или BasedProductId
        CollectRelatedOperations(mainOperation, operationsById, result, visited);

        return result;
    }

    private static void CollectRelatedOperations(
        OperationDto operation,
        Dictionary<int, OperationDto> operationsById,
        List<OperationDto> result,
        HashSet<int> visited)
    {
        // Если операция основана на другой операции, добавляем эту операцию и её связанные
        if (operation.BasedOnType == OperationBasedOnType.Operation && operation.BasedOperationId.HasValue)
        {
            var basedOperationId = operation.BasedOperationId.Value;
            if (!visited.Contains(basedOperationId) &&
                operationsById.TryGetValue(basedOperationId, out var basedOperation))
            {
                visited.Add(basedOperationId);
                result.Add(basedOperation);
                CollectRelatedOperations(basedOperation, operationsById, result, visited);
            }
        }

        // Если операция связана с продуктом, собираем все операции, связанные с тем же продуктом
        if (operation.BasedOnType == OperationBasedOnType.Product && operation.BasedProductId.HasValue)
        {
            var productRelatedOperations = operationsById.Values
                .Where(op => !visited.Contains(op.Id) &&
                             op.BasedOnType == OperationBasedOnType.Product &&
                             op.BasedProductId == operation.BasedProductId);

            foreach (var productOp in productRelatedOperations)
            {
                visited.Add(productOp.Id);
                result.Add(productOp);
                CollectRelatedOperations(productOp, operationsById, result, visited);
            }
        }

        // Собираем операции, которые основаны на текущей операции
        var dependentOperations = operationsById.Values
            .Where(op => !visited.Contains(op.Id) &&
                         op.BasedOnType == OperationBasedOnType.Operation &&
                         op.BasedOperationId == operation.Id);

        foreach (var dependentOp in dependentOperations)
        {
            visited.Add(dependentOp.Id);
            result.Add(dependentOp);
            CollectRelatedOperations(dependentOp, operationsById, result, visited);
        }
    }

    private static double CalculateCycleDuration(ICollection<OperationDto> operations)
    {
        // Суммируем длительность всех операций в цикле
        return operations
            .Where(op => op.Duration.HasValue)
            .Sum(op => op.Duration.Value.TotalSeconds);
    }

    private short ProcessBreakIntervalForOperations(
        List<FormRowData> rows,
        short order,
        ShiftScheduleDto nextBreak,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperationsByIds,
        InitializedIndicators indicators,
        ref int breakIndex,
        ref TimeOnly currentTime,
        ref TimeSpan elapsedWorkTime)
    {
        if (currentTime < nextBreak.StartTime)
        {
            // Не должно быть работы перед перерывом в режиме операций, но на всякий случай
            elapsedWorkTime = elapsedWorkTime.Add(nextBreak.StartTime - currentTime);
        }

        var breakMetaInfo = auxiliaryOperationsByIds[nextBreak.AuxiliaryOperationId];
        var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

        var breakRow = formRowDataFactory.CreateBreakRow(
            order++,
            indicators.WorkTime,
            nextBreak.StartTime,
            breakEndTime,
            breakMetaInfo.Name,
            nextBreak.AuxiliaryOperationId);

        rows.Add(breakRow);

        currentTime = breakEndTime;
        elapsedWorkTime = elapsedWorkTime.Add(breakMetaInfo.Duration);
        breakIndex++;

        return order;
    }

    private record InitializedIndicators
    {
        public required Indicator WorkTime { get; init; }
        public Indicator? Plan { get; init; }
        public Indicator? OperationName { get; init; }
        public Indicator? OperationTime { get; init; }
    }
}