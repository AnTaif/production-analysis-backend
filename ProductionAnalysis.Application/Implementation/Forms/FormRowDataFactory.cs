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

    FormRowData CreateOperationCycleRow(
        short order,
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

    public FormRowData CreateOperationCycleRow(
        short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        Indicator? operationNameIndicator,
        Indicator? operationTimeIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ICollection<OperationDto> operations)
    {
        var values = new List<FormRowValueData>
        {
            CreateFormRowValueData(workTimeIndicator, FormatTimeRange(startTime, endTime))
        };

        if (planIndicator is not null)
        {
            // В плане всегда 1 шт. в рамках полного цикла под-операций
            values.Add(CreateFormRowValueData(planIndicator, "1"));
        }

        // Добавляем наименования операций
        if (operationNameIndicator is not null)
        {
            var operationNames = string.Join("\n", operations.Select((op, index) => $"{index + 1}. {op.Name}"));
            values.Add(CreateFormRowValueData(operationNameIndicator, operationNames));
        }

        // Добавляем время каждой операции в минутах
        if (operationTimeIndicator is not null)
        {
            var operationTimes = string.Join("\n", operations.Select(op =>
                op.Duration.HasValue ? (op.Duration.Value.TotalMinutes).ToString("0") : "-"));
            values.Add(CreateFormRowValueData(operationTimeIndicator, operationTimes));
        }

        return new FormRowData
        {
            Order = order,
            IsAuxiliaryOperation = false,
            Values = values
        };
    }

    private static FormRowValueData CreateFormRowValueData(Indicator indicator, string value) =>
        new()
        {
            IndicatorId = indicator.Id,
            Value = value
        };
}