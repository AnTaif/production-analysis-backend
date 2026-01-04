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
        Dictionary<string, FormContext>? formContext = null);
}

[RegisterScoped]
public class FormRowFormulaCalculator(IFormulaCalculator formulaCalculator) : IFormRowFormulaCalculator
{
    public Task<ICollection<FormRowValueData>> CalculateFormulaValuesAsync(
        FormRow row,
        Template template,
        ICollection<int> updatedIndicatorIds,
        Dictionary<string, FormContext>? formContext = null)
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
        return ValueComparer.AreEqual(value1, value2);
    }
}