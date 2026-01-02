using System.Globalization;
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
            .OrderBy(r => r.Order)
            .Take(toRowOrder)
            .ToList();

        var valuesToUpdateByRow = new Dictionary<short, ICollection<FormRowValueData>>();

        foreach (var row in sortedRows)
        {
            var rowValues = new List<FormRowValueData>();

            foreach (var indicator in cumulativeIndicators)
            {
                if (!row.Values.TryGetValue(indicator.Id.ToString(), out var rowValue) && rowValue is null)
                {
                    continue;
                }

                var cumulativeValue = CalculateCumulativeValueForRow(
                    indicator.Id,
                    row.Order,
                    sortedRows);

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

    private static object CalculateCumulativeValueForRow(
        int indicatorId,
        short currentRowOrder,
        ICollection<FormRow> allRows)
    {
        var rowsUpToCurrent = allRows
            .Where(r => r.Order <= currentRowOrder)
            .OrderBy(r => r.Order)
            .ToList();

        double cumulativeSum = 0;

        foreach (var row in rowsUpToCurrent)
        {
            var value = GetIndicatorValue(row, indicatorId);
            if (TryConvertToDouble(value, out var numValue))
            {
                cumulativeSum += numValue;
            }
        }

        if (Math.Abs(cumulativeSum - Math.Round(cumulativeSum)) < 0.0001)
        {
            return (int)Math.Round(cumulativeSum);
        }

        return cumulativeSum;
    }

    private static object? GetIndicatorValue(FormRow row, int indicatorId)
    {
        return row.Values.TryGetValue(indicatorId.ToString(), out var rowValue)
            ? rowValue.Value
            : null;
    }

    private static bool TryConvertToDouble(object? value, out double result)
    {
        result = 0;
        if (value == null)
        {
            return false;
        }

        switch (value)
        {
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case double d:
                result = d;
                return true;
            case decimal dec:
                result = (double)dec;
                return true;
            case float f:
                result = f;
                return true;
            case string s:
                return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            default:
                return false;
        }
    }
}