using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface ICumulativeValueCalculator
{
    Dictionary<short, ICollection<FormRowValueData>> CalculateCumulativeValues(
        Form form,
        short fromRowOrder);

    void CalculateCumulativeValuesForFormRowData(
        ICollection<FormRowData> rows,
        ICollection<Indicator> indicators);
}

[RegisterScoped]
public class CumulativeValueCalculator : ICumulativeValueCalculator
{
    public Dictionary<short, ICollection<FormRowValueData>> CalculateCumulativeValues(
        Form form,
        short fromRowOrder)
    {
        var cumulativeIndicators = form.TemplateSnapshot.Indicators
            .Where(i => i.IsCumulative)
            .ToList();

        if (cumulativeIndicators.Count == 0)
        {
            return new Dictionary<short, ICollection<FormRowValueData>>();
        }

        var sortedRows = form.Rows
            .Where(r => !r.IsAdditionalOperation)
            .Where(r => r.Order >= fromRowOrder)
            .OrderBy(r => r.Order)
            .ToList();

        var valuesToUpdateByRow = new Dictionary<short, ICollection<FormRowValueData>>();

        for (var i = 0; i < sortedRows.Count; i++)
        {
            var row = sortedRows[i];
            var rowValues = new List<FormRowValueData>();

            foreach (var indicator in cumulativeIndicators)
            {
                if (!row.Values.TryGetValue(indicator.Id.ToString(), out var rowValue))
                {
                    continue;
                }

                var cumulativeValue = CalculateCumulativeValueForFormRow(indicator.Id, i, sortedRows);

                if (cumulativeValue == null)
                {
                    continue;
                }

                rowValues.Add(new FormRowValueData
                {
                    IndicatorId = indicator.Id,
                    Value = rowValue.Value,
                    CumulativeValue = cumulativeValue
                });
            }

            if (rowValues.Count > 0)
            {
                valuesToUpdateByRow[row.Order] = rowValues;
            }
        }

        return valuesToUpdateByRow;
    }

    public void CalculateCumulativeValuesForFormRowData(
        ICollection<FormRowData> rows,
        ICollection<Indicator> indicators)
    {
        var cumulativeIndicators = indicators
            .Where(i => i.IsCumulative)
            .ToList();

        if (indicators.Count == 0)
        {
            return;
        }

        var workRows = rows
            .Where(r => !r.IsAdditionalOperation)
            .OrderBy(r => r.Order)
            .ToList();

        var previousCumulativeValues = new Dictionary<int, int>();

        for (var i = 0; i < workRows.Count; i++)
        {
            var row = workRows[i];

            foreach (var indicator in cumulativeIndicators)
            {
                var valueData = row.Values.FirstOrDefault(v => v.IndicatorId == indicator.Id);
                if (valueData == null)
                {
                    continue;
                }

                if (!int.TryParse(valueData.Value.ToString(), out var baseValue))
                {
                    continue;
                }

                var cumulativeValue = i == 0
                    ? baseValue
                    : previousCumulativeValues.GetValueOrDefault(indicator.Id, 0) + baseValue;

                valueData.CumulativeValue = cumulativeValue;
                previousCumulativeValues[indicator.Id] = cumulativeValue;
            }
        }
    }

    private static int? CalculateCumulativeValueForFormRow(
        int indicatorId,
        int currentRowIndex,
        IList<FormRow> sortedRows)
    {
        var currentRow = sortedRows[currentRowIndex];

        if (!currentRow.Values.TryGetValue(indicatorId.ToString(), out var currentRowValue))
        {
            return null;
        }

        if (!int.TryParse(currentRowValue.Value.ToString(), out var rowValue))
        {
            return null;
        }

        if (currentRowIndex == 0)
        {
            return rowValue;
        }

        var previousRow = sortedRows[currentRowIndex - 1];

        if (!previousRow.Values.TryGetValue(indicatorId.ToString(), out var previousRowValue))
        {
            return null;
        }

        if (!int.TryParse(previousRowValue.CumulativeValue?.ToString(), out var previousCumulativeValue))
        {
            return null;
        }

        return previousCumulativeValue + rowValue;
    }
}