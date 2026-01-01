using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;

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
        int additionalOperationId);
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
            IsAdditionalOperation = false,
            Values = values
        };
    }

    public FormRowData CreateBreakRow(
        short order,
        Indicator workTimeIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        string operationName,
        int additionalOperationId)
    {
        var values = new List<FormRowValueData>
        {
            CreateFormRowValueData(workTimeIndicator, FormatTimeRange(startTime, endTime) + " " + operationName)
        };

        return new FormRowData
        {
            Order = order,
            IsAdditionalOperation = true,
            AdditionalOperationId = additionalOperationId,
            Values = values
        };
    }

    private static string FormatTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        return $"{startTime:HH:mm}-{endTime:HH:mm}";
    }

    private static FormRowValueData CreateFormRowValueData(Indicator indicator, string value) =>
        new()
        {
            IndicatorId = indicator.Id,
            Value = value
        };
}