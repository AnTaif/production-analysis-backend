using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowDataFactory
{
    FormRowData CreateWorkRow(
        short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ProductContext? productContext);

    FormRowData CreateBreakRow(
        short order,
        Indicator workTimeIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        string operationName,
        int auxiliaryOperationId);

    ICollection<FormRowData> CreateOperationCycleRows(
        ref short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        Indicator? operationNameIndicator,
        Indicator? operationTimeIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ICollection<OperationDto> operations);
}

[RegisterScoped]
public class FormRowDataFactory(IPlanCalculator planCalculator) : IFormRowDataFactory
{
    public FormRowData CreateWorkRow(
        short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ProductContext? productContext)
    {
        var values = new List<FormRowValueData>
        {
            CreateFormRowValueData(workTimeIndicator, FormatTimeRange(startTime, endTime))
        };

        if (planIndicator is not null)
        {
            if (productContext is null)
            {
                throw new ArgumentNullException(nameof(productContext), "ProductContext cannot be null");
            }

            var planValue = planCalculator.Calculate(startTime, endTime, productContext);
            values.Add(CreateFormRowValueData(planIndicator, planValue.ToString()));
        }

        return new FormRowData
        {
            Order = order,
            IsAuxiliaryOperation = false,
            ProductId = productContext?.ProductId,
            Values = values
        };
    }

    public FormRowData CreateBreakRow(
        short order,
        Indicator workTimeIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        string operationName,
        int auxiliaryOperationId)
    {
        var values = new List<FormRowValueData>
        {
            CreateFormRowValueData(workTimeIndicator, FormatTimeRange(startTime, endTime) + " " + operationName)
        };

        return new FormRowData
        {
            Order = order,
            IsAuxiliaryOperation = true,
            AuxiliaryOperationId = auxiliaryOperationId,
            Values = values
        };
    }

    private static string FormatTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        return $"{startTime:HH:mm}-{endTime:HH:mm}";
    }

    public ICollection<FormRowData> CreateOperationCycleRows(
        ref short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        Indicator? operationNameIndicator,
        Indicator? operationTimeIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ICollection<OperationDto> operations)
    {
        var rows = new List<FormRowData>();
        var workTimeValue = FormatTimeRange(startTime, endTime);
        var planValue = "1"; // В плане всегда 1 шт. в рамках полного цикла под-операций

        // Генерируем уникальный ключ группы для группировки строк
        var groupKey = order;

        // Создаем отдельную строку для каждой под-операции
        var operationList = operations.ToList();
        for (var i = 0; i < operationList.Count; i++)
        {
            var operation = operationList[i];
            var values = new List<FormRowValueData>
            {
                CreateFormRowValueData(workTimeIndicator, workTimeValue)
            };

            // План одинаковый для всех строк цикла
            if (planIndicator is not null)
            {
                values.Add(CreateFormRowValueData(planIndicator, planValue));
            }

            // Наименование операции - только для текущей операции
            if (operationNameIndicator is not null)
            {
                var operationName = $"{i + 1}. {operation.Name}";
                values.Add(CreateFormRowValueData(operationNameIndicator, operationName));
            }

            // Время операции - только для текущей операции
            if (operationTimeIndicator is not null)
            {
                var operationTime = operation.Duration.HasValue
                    ? operation.Duration.Value.TotalMinutes.ToString("0")
                    : "-";
                values.Add(CreateFormRowValueData(operationTimeIndicator, operationTime));
            }

            rows.Add(new FormRowData
            {
                Order = order++,
                IsAuxiliaryOperation = false,
                GroupKey = groupKey, // Все строки одной группы имеют одинаковый GroupKey
                Values = values
            });
        }

        return rows;
    }

    private static FormRowValueData CreateFormRowValueData(Indicator indicator, string value) =>
        new()
        {
            IndicatorId = indicator.Id,
            Value = value
        };
}