using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
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
    IProductContextExtractor productContextExtractor,
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
        var additionalOperationsByIds = await LoadAdditionalOperationsAsync();
        var sortedBreaks = schedules.OrderBy(s => s.StartTime).ToList();

        // Проверяем, есть ли несколько продуктов
        var multiProducts = multiProductContextExtractor.Extract(formContext);
        if (multiProducts.Count > 1)
        {
            return await InitializeRowsForMultipleProductsAsync(
                shiftStartTime,
                sortedBreaks,
                template,
                indicators,
                additionalOperationsByIds,
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
            additionalOperationsByIds,
            productContext,
            productContext.ProductId,
            ref order);

        cumulativeValueCalculator.FillCumulativeValues(rows, template.Indicators);

        return rows;
    }

    private async Task<ICollection<FormRowData>> InitializeRowsForMultipleProductsAsync(
        TimeOnly shiftStartTime,
        List<ShiftScheduleDto> sortedBreaks,
        Template template,
        InitializedIndicators indicators,
        Dictionary<int, AdditionalOperationDto> additionalOperationsByIds,
        Dictionary<string, FormContext>? formContext)
    {
        var multiProducts = multiProductContextExtractor.Extract(formContext);
        var allRows = new List<FormRowData>();
        short globalOrder = 1;

        // Получаем информацию о продуктах из контекста
        var productInfos = new List<(int? ProductId, ProductInfo ProductInfo)>();
        foreach (var (_, context) in formContext ?? new Dictionary<string, FormContext>())
        {
            if (context is MultiProductContext multiProductContext)
            {
                foreach (var productInfo in multiProductContext.Products)
                {
                    productInfos.Add((productInfo.ProductId, productInfo));
                }
            }
        }

        foreach (var (productId, productInfo) in productInfos)
        {
            // Преобразуем ProductInfo в ProductContext для расчетов
            var productContext = new ProductContext(
                productInfo.ProductId,
                productInfo.CycleTime,
                productInfo.WorkstationCapacity,
                productInfo.DailyRate);

            var productRows = InitializeRowsForSingleProduct(
                shiftStartTime,
                sortedBreaks,
                indicators,
                additionalOperationsByIds,
                productContext,
                productId,
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
        Dictionary<int, AdditionalOperationDto> additionalOperationsByIds,
        ProductContext? productContext,
        int? productId,
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
                    additionalOperationsByIds,
                    indicators,
                    productContext,
                    productId,
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
                    productContext,
                    productId);

                rows.Add(workRow);
                currentTime = workIntervalEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(workIntervalDuration);
            }
        }

        // Если чистое рабочее время закончилось, но остались перерывы - в цикле добавляем их в расписание
        while (breakIndex < sortedBreaks.Count)
        {
            var nextBreak = sortedBreaks[breakIndex];
            var breakMetaInfo = additionalOperationsByIds[nextBreak.AdditionalOperationId];
            var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

            var breakRow = formRowDataFactory.CreateBreakRow(
                localOrder++,
                indicators.WorkTime,
                nextBreak.StartTime,
                breakEndTime,
                breakMetaInfo.Name,
                nextBreak.AdditionalOperationId);

            rows.Add(breakRow);
            breakIndex++;
        }

        order = localOrder;
        return rows;
    }

    private async Task<Dictionary<int, AdditionalOperationDto>> LoadAdditionalOperationsAsync()
    {
        var operations = await unitOfWork.Dictionaries.SelectAdditionalOperationsAsync();
        return operations.ToDictionary(ao => ao.Id);
    }

    private static InitializedIndicators ExtractIndicators(Template template)
    {
        return new InitializedIndicators
        {
            WorkTime = template.Indicators.Single(i => i.Id == ShiftConstants.WorktimeIndicatorId),
            Plan = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.PlanIndicatorId)
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
        Dictionary<int, AdditionalOperationDto> additionalOperationsByIds,
        InitializedIndicators indicators,
        ProductContext? productContext,
        int? productId,
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
                productContext,
                productId);

            rows.Add(workRowBeforeBreak);

            elapsedWorkTime = elapsedWorkTime.Add(workDuration);
        }

        var breakMetaInfo = additionalOperationsByIds[nextBreak.AdditionalOperationId];
        var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

        var breakRow = formRowDataFactory.CreateBreakRow(
            order++,
            indicators.WorkTime,
            nextBreak.StartTime,
            breakEndTime,
            breakMetaInfo.Name,
            nextBreak.AdditionalOperationId);

        rows.Add(breakRow);

        currentTime = breakEndTime;
        breakIndex++;

        return order;
    }

    private record InitializedIndicators
    {
        public required Indicator WorkTime { get; init; }
        public Indicator? Plan { get; init; }
    }
}