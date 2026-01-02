using System.Globalization;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowFormulaCalculator
{
    Task<ICollection<FormRowValueData>> CalculateFormulaValuesAsync(
        FormRow row,
        Template template,
        ICollection<int> updatedIndicatorIds,
        Dictionary<string, FormContextBase>? formContext = null);
}

[RegisterScoped]
public class FormRowFormulaCalculator(IFormulaCalculator formulaCalculator) : IFormRowFormulaCalculator
{
    public Task<ICollection<FormRowValueData>> CalculateFormulaValuesAsync(
        FormRow row,
        Template template,
        ICollection<int> updatedIndicatorIds,
        Dictionary<string, FormContextBase>? formContext = null)
    {
        var currentValues = ParseRowValuesToDictionary(row.Values);
        var calculatedValues = formulaCalculator.CalculateFormulas(
            currentValues,
            template.Indicators,
            updatedIndicatorIds,
            formContext);

        var formulaValuesToUpdate = new List<FormRowValueData>();
        foreach (var (indicatorId, calculatedValue) in calculatedValues)
        {
            if (currentValues.TryGetValue(indicatorId, out var oldValue) && AreValuesEqual(oldValue, calculatedValue))
            {
                continue;
            }

            var formulaIndicator = template.Indicators.FirstOrDefault(i => i.Id == indicatorId);
            if (formulaIndicator is { InputType: FieldInputTypes.Formula })
            {
                formulaValuesToUpdate.Add(new FormRowValueData
                {
                    IndicatorId = indicatorId,
                    Value = calculatedValue
                });
            }
        }

        return Task.FromResult<ICollection<FormRowValueData>>(formulaValuesToUpdate);
    }

    private static Dictionary<int, object> ParseRowValuesToDictionary(Dictionary<string, FormRowValue> rowValues)
    {
        var result = new Dictionary<int, object>();
        foreach (var (key, rowValue) in rowValues)
        {
            if (int.TryParse(key, out var indicatorId))
            {
                result[indicatorId] = rowValue.Value;
            }
        }

        return result;
    }

    private static bool AreValuesEqual(object? value1, object? value2)
    {
        if (value1 == null && value2 == null)
        {
            return true;
        }

        if (value1 == null || value2 == null)
        {
            return false;
        }

        if (TryConvertToDouble(value1, out var num1) && TryConvertToDouble(value2, out var num2))
        {
            return Math.Abs(num1 - num2) < 0.0001;
        }

        return value1.Equals(value2);
    }

    private static bool TryConvertToDouble(object value, out double result)
    {
        result = 0;
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