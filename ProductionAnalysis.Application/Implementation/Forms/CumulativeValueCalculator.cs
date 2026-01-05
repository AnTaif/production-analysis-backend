using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface ICumulativeValueCalculator
{
    Dictionary<short, ICollection<FormRowValueData>> CalculateCumulativeValues(Form form, short fromRowOrder);
    void FillCumulativeValues(ICollection<FormRowData> rows, ICollection<Indicator> indicators);
}

[RegisterScoped]
public class CumulativeValueCalculator : ICumulativeValueCalculator
{
    public Dictionary<short, ICollection<FormRowValueData>> CalculateCumulativeValues(Form form, short fromRowOrder)
    {
        var cumulativeIndicators = GetCumulativeIndicators(form.TemplateSnapshot.Indicators);
        if (cumulativeIndicators.Count == 0)
        {
            return new Dictionary<short, ICollection<FormRowValueData>>();
        }

        var updatedRow = GetWorkRows(form.Rows).FirstOrDefault(r => r.Order == fromRowOrder);
        if (updatedRow == null)
        {
            return new Dictionary<short, ICollection<FormRowValueData>>();
        }

        // Получаем все строки того же продукта, начиная с обновленной строки
        var productId = updatedRow.ProductId;
        var workRows = GetWorkRows(form.Rows)
            .Where(r => r.ProductId == productId && r.Order >= fromRowOrder)
            .OrderBy(r => r.Order)
            .ToList();

        if (workRows.Count == 0)
        {
            return new Dictionary<short, ICollection<FormRowValueData>>();
        }

        // Находим предыдущую строку того же продукта
        var previousWorkRow = FindPreviousWorkRow(form.Rows, fromRowOrder, productId);
        var cumulativeValuesByIndicator = BuildInitialCumulativeValues(previousWorkRow, cumulativeIndicators);
        var valuesToUpdateByRow = new Dictionary<short, ICollection<FormRowValueData>>();

        foreach (var row in workRows)
        {
            var rowValues = CalculateRowCumulativeValues(
                row,
                cumulativeIndicators,
                cumulativeValuesByIndicator);

            if (rowValues.Count > 0)
            {
                valuesToUpdateByRow[row.Order] = rowValues;
            }
        }

        return valuesToUpdateByRow;
    }

    public void FillCumulativeValues(ICollection<FormRowData> rows, ICollection<Indicator> indicators)
    {
        var cumulativeIndicators = GetCumulativeIndicators(indicators);
        if (cumulativeIndicators.Count == 0)
        {
            return;
        }

        // Группируем строки по продуктам для отдельного расчета накопительных значений
        var rowsByProduct = rows
            .Where(r => !r.IsAuxiliaryOperation)
            .GroupBy(r => r.ProductId)
            .ToList();

        foreach (var productGroup in rowsByProduct)
        {
            var workRows = productGroup.OrderBy(r => r.Order).ToList();
            var cumulativeValuesByIndicator = new Dictionary<int, int>();

            foreach (var row in workRows)
            {
                foreach (var indicator in cumulativeIndicators)
                {
                    var valueData = row.Values.FirstOrDefault(v => v.IndicatorId == indicator.Id);
                    if (valueData == null)
                    {
                        continue;
                    }

                    if (!TryParseIntValue(valueData.Value, out var baseValue))
                    {
                        continue;
                    }

                    var previousCumulative = cumulativeValuesByIndicator.GetValueOrDefault(indicator.Id, 0);
                    var cumulativeValue = previousCumulative + baseValue;

                    valueData.CumulativeValue = cumulativeValue;
                    cumulativeValuesByIndicator[indicator.Id] = cumulativeValue;
                }
            }
        }
    }

    private static IList<Indicator> GetCumulativeIndicators(ICollection<Indicator> indicators)
    {
        return indicators
            .Where(i => i.IsCumulative)
            .ToList();
    }

    private static IEnumerable<FormRow> GetWorkRows(ICollection<FormRow> rows)
    {
        return rows.Where(r => !r.IsAuxiliaryOperation);
    }

    private static FormRow? FindPreviousWorkRow(ICollection<FormRow> rows, short fromRowOrder, int? productId)
    {
        if (fromRowOrder <= 1)
        {
            return null;
        }

        return GetWorkRows(rows)
            .Where(r => r.ProductId == productId && r.Order < fromRowOrder)
            .OrderByDescending(r => r.Order)
            .FirstOrDefault();
    }

    private static Dictionary<int, int> BuildInitialCumulativeValues(
        FormRow? previousWorkRow,
        IList<Indicator> cumulativeIndicators)
    {
        var initialValues = new Dictionary<int, int>();

        if (previousWorkRow == null)
        {
            return initialValues;
        }

        foreach (var indicator in cumulativeIndicators)
        {
            var indicatorKey = indicator.Id.ToString();
            if (!previousWorkRow.Values.TryGetValue(indicatorKey, out var previousValue))
            {
                continue;
            }

            if (TryParseIntValue(previousValue.CumulativeValue, out var cumulativeValue))
            {
                initialValues[indicator.Id] = cumulativeValue;
            }
        }

        return initialValues;
    }

    private static List<FormRowValueData> CalculateRowCumulativeValues(
        FormRow row,
        IList<Indicator> cumulativeIndicators,
        Dictionary<int, int> cumulativeValuesByIndicator)
    {
        var rowValues = new List<FormRowValueData>();

        foreach (var indicator in cumulativeIndicators)
        {
            var indicatorKey = indicator.Id.ToString();
            if (!row.Values.TryGetValue(indicatorKey, out var rowValue))
            {
                continue;
            }

            if (!TryParseIntValue(rowValue.Value, out var baseValue))
            {
                continue;
            }

            var previousCumulative = cumulativeValuesByIndicator.GetValueOrDefault(indicator.Id, 0);
            var cumulativeValue = previousCumulative + baseValue;

            rowValues.Add(new FormRowValueData
            {
                IndicatorId = indicator.Id,
                Value = rowValue.Value,
                CumulativeValue = cumulativeValue
            });

            cumulativeValuesByIndicator[indicator.Id] = cumulativeValue;
        }

        return rowValues;
    }

    private static bool TryParseIntValue(object? value, out int result)
    {
        result = 0;

        if (value == null)
        {
            return false;
        }

        return int.TryParse(value.ToString(), out result);
    }
}