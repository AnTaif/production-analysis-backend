using System.Globalization;
using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface ITotalValueCalculator
{
    Dictionary<int, object> CalculateTotals(Form form);
}

[RegisterScoped]
public class TotalValueCalculator : ITotalValueCalculator
{
    public Dictionary<int, object> CalculateTotals(Form form)
    {
        var totals = new Dictionary<int, object>();

        // Получаем индикаторы, для которых нужно вычислять итоги
        var summationIndicators = form.TemplateSnapshot.Indicators
            .Where(i => i.HasSummation)
            .ToList();

        if (summationIndicators.Count == 0) return totals;

        // Получаем только рабочие строки (не дополнительные операции)
        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .ToList();

        // Для каждого индикатора с суммированием вычисляем итог
        foreach (var indicator in summationIndicators)
        {
            var indicatorKey = indicator.Id.ToString();
            var total = 0.0;

            foreach (var row in workRows)
            {
                if (!row.Values.TryGetValue(indicatorKey, out var rowValue)) continue;

                // Пытаемся преобразовать значение в число
                if (TryParseNumericValue(rowValue.Value, out var numericValue)) total += numericValue;
            }

            // Сохраняем итог как int, если это целое число, иначе как double
            totals[indicator.Id] = Math.Abs(total % 1) < 0.0001 ? (object)(int)total : total;
        }

        return totals;
    }

    private static bool TryParseNumericValue(object? value, out double result)
    {
        result = 0;

        if (value == null) return false;

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
                return double.TryParse(s, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out result);
            default:
                return false;
        }
    }
}