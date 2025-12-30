using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormulaCalculator
{
    Dictionary<int, object> CalculateFormulas(
        Dictionary<int, object> currentValues,
        ICollection<Indicator> indicators,
        ICollection<int> updatedIndicatorIds);
}

[RegisterScoped]
public class FormulaCalculator : IFormulaCalculator
{
    private static readonly Regex IndicatorReferenceRegex = new(@"indicator_(\d+)", RegexOptions.Compiled);

    public Dictionary<int, object> CalculateFormulas(
        Dictionary<int, object> currentValues,
        ICollection<Indicator> indicators,
        ICollection<int> updatedIndicatorIds)
    {
        var formulaIndicators = indicators
            .Where(i => i.InputType == FieldInputTypes.Formula && !string.IsNullOrEmpty(i.Formula))
            .ToList();

        var calculatedValues = new Dictionary<int, object>(currentValues);
        var processedIndicators = new HashSet<int>();

        // Находим все формулы, которые зависят от обновленных индикаторов
        var dependentFormulas = FindDependentFormulas(formulaIndicators, updatedIndicatorIds, processedIndicators);

        // Вычисляем формулы в порядке зависимостей
        while (dependentFormulas.Any())
        {
            var formulaToCalculate = dependentFormulas
                .FirstOrDefault(f => CanCalculateFormula(f, calculatedValues, processedIndicators));

            if (formulaToCalculate == null)
            {
                // Если не можем вычислить формулу (циклическая зависимость или отсутствующие значения), пропускаем
                break;
            }

            var calculatedValue = EvaluateFormula(formulaToCalculate.Formula!, calculatedValues);
            if (calculatedValue != null)
            {
                calculatedValues[formulaToCalculate.Id] = calculatedValue;
                processedIndicators.Add(formulaToCalculate.Id);

                // Находим формулы, которые зависят от только что вычисленной
                var nextDependentFormulas = FindDependentFormulas(
                    formulaIndicators.Where(i => !processedIndicators.Contains(i.Id)).ToList(),
                    new[] { formulaToCalculate.Id },
                    processedIndicators);

                foreach (var nextFormula in nextDependentFormulas)
                {
                    if (!dependentFormulas.Contains(nextFormula))
                    {
                        dependentFormulas.Add(nextFormula);
                    }
                }
            }

            dependentFormulas.Remove(formulaToCalculate);
        }

        return calculatedValues;
    }

    private List<Indicator> FindDependentFormulas(
        ICollection<Indicator> formulaIndicators,
        ICollection<int> sourceIndicatorIds,
        HashSet<int> processedIndicators)
    {
        var dependentFormulas = new List<Indicator>();

        foreach (var formulaIndicator in formulaIndicators)
        {
            if (processedIndicators.Contains(formulaIndicator.Id))
            {
                continue;
            }

            if (string.IsNullOrEmpty(formulaIndicator.Formula))
            {
                continue;
            }

            // Проверяем, зависит ли формула от указанных индикаторов
            var referencedIndicators = ExtractIndicatorReferences(formulaIndicator.Formula);
            if (referencedIndicators.Any(id => sourceIndicatorIds.Contains(id)))
            {
                dependentFormulas.Add(formulaIndicator);
            }
        }

        return dependentFormulas;
    }

    private bool CanCalculateFormula(Indicator formulaIndicator, Dictionary<int, object> currentValues,
        HashSet<int> processedIndicators)
    {
        if (string.IsNullOrEmpty(formulaIndicator.Formula))
        {
            return false;
        }

        var referencedIndicators = ExtractIndicatorReferences(formulaIndicator.Formula);

        // Проверяем, что все необходимые значения доступны
        return referencedIndicators.All(id => currentValues.ContainsKey(id) || processedIndicators.Contains(id));
    }

    private object? EvaluateFormula(string formula, Dictionary<int, object> currentValues)
    {
        try
        {
            // Заменяем ссылки на индикаторы на их значения
            var expression = IndicatorReferenceRegex.Replace(formula, match =>
            {
                var indicatorId = int.Parse(match.Groups[1].Value);
                if (currentValues.TryGetValue(indicatorId, out var value))
                {
                    return ConvertToNumericString(value);
                }

                return "0";
            });

            // Вычисляем выражение
            return EvaluateExpression(expression);
        }
        catch
        {
            return null;
        }
    }

    private ICollection<int> ExtractIndicatorReferences(string formula)
    {
        var matches = IndicatorReferenceRegex.Matches(formula);
        return matches
            .Select(m => int.Parse(m.Groups[1].Value))
            .Distinct()
            .ToList();
    }

    private string ConvertToNumericString(object value)
    {
        return value switch
        {
            int i => i.ToString(),
            long l => l.ToString(),
            double d => d.ToString("G", CultureInfo.InvariantCulture),
            decimal dec => dec.ToString("G", CultureInfo.InvariantCulture),
            float f => f.ToString("G", CultureInfo.InvariantCulture),
            string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var num) =>
                num.ToString("G", CultureInfo.InvariantCulture),
            _ => "0"
        };
    }

    private object? EvaluateExpression(string expression)
    {
        try
        {
            // Используем DataTable для безопасного вычисления выражений
            var dataTable = new DataTable();
            var result = dataTable.Compute(expression, null);

            if (result == DBNull.Value)
            {
                return null;
            }

            return result;
        }
        catch
        {
            return null;
        }
    }
}