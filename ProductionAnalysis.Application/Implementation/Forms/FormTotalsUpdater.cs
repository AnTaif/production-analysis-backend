using System.Globalization;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Repositories;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormTotalsUpdater
{
    Task UpdateTotalsIfNeededAsync(Form form, Guid userId);
}

[RegisterScoped]
public class FormTotalsUpdater(
    IPaUnitOfWork unitOfWork
) : IFormTotalsUpdater
{
    public async Task UpdateTotalsIfNeededAsync(Form form, Guid userId)
    {
        var calculatedTotals = CalculateTotals(form);

        var needsUpdate = calculatedTotals.Count > 0 &&
                          (form.TotalValues == null
                           || !ValueComparer.AreDictionariesEqual(form.TotalValues, calculatedTotals));

        if (needsUpdate)
        {
            await unitOfWork.Forms.UpdateTotalValuesAsync(form.Id, calculatedTotals, userId);
        }
    }

    private static Dictionary<int, object> CalculateTotals(Form form)
    {
        var totals = new Dictionary<int, object>();

        var summationIndicators = form.TemplateSnapshot.Indicators
            .Where(i => i.HasSummation)
            .ToList();

        if (summationIndicators.Count == 0)
        {
            return totals;
        }

        var workRows = form.Rows
            .Where(r => !r.IsAuxiliaryOperation)
            .OrderBy(r => r.Order)
            .ToArray();

        foreach (var indicator in summationIndicators)
        {
            var total = GetTotalForIndicator(indicator.Id.ToString(), workRows);
            totals[indicator.Id] = Math.Abs(total % 1) < 0.0001 ? (object)(int)total : total;
        }

        return totals;
    }

    private static double GetTotalForIndicator(string indicatorKey, IEnumerable<FormRow> workRows)
    {
        var total = 0.0;

        foreach (var row in workRows)
        {
            if (!row.Values.TryGetValue(indicatorKey, out var rowValue))
            {
                continue;
            }

            if (TryParseNumericValue(rowValue.Value, out var numericValue))
            {
                total += numericValue;
            }
        }

        return total;
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