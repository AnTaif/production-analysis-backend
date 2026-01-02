using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface ICumulativeValueCalculator
{
    Dictionary<short, ICollection<FormRowValueData>> CalculateCumulativeValues(
        Form form,
        short fromRowOrder);
}

[RegisterScoped]
public class CumulativeValueCalculator : ICumulativeValueCalculator
{
    public Dictionary<short, ICollection<FormRowValueData>> CalculateCumulativeValues(
        Form form,
        short toRowOrder)
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
            .OrderBy(r => r.Order)
            .Take(toRowOrder)
            .ToList();

        var valuesToUpdateByRow = new Dictionary<short, ICollection<FormRowValueData>>();

        for (var i = 0; i < sortedRows.Count; i++)
        {
            var row = sortedRows[i];
            var rowValues = new List<FormRowValueData>();

            foreach (var indicator in cumulativeIndicators)
            {
                if (!row.Values.TryGetValue(indicator.Id.ToString(), out var rowValue) && rowValue is null)
                {
                    continue;
                }

                var cumulativeValue = CalculateCumulativeValueForRow(indicator.Id, i, sortedRows);

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

    private static int? CalculateCumulativeValueForRow(
        int indicatorId,
        int currentRowIndex,
        IList<FormRow> sortedRows)
    {
        var currentRow = sortedRows[currentRowIndex];
        var currentRowValue = currentRow.Values[indicatorId.ToString()];

        if (!int.TryParse(currentRowValue.Value.ToString(), out var rowValue))
        {
            return null;
        }

        if (currentRowIndex == 0)
        {
            return rowValue;
        }

        var previousRow = sortedRows[currentRowIndex - 1];
        var previousRowValue = previousRow.Values[indicatorId.ToString()];

        if (!int.TryParse(previousRowValue.CumulativeValue?.ToString(), out var cumulativeValue))
        {
            return null;
        }

        return cumulativeValue + rowValue;
    }
}