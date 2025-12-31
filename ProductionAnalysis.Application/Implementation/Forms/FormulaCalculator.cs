using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormulaCalculator
{
    Dictionary<int, object> CalculateFormulas(
        Dictionary<int, object> currentValues,
        ICollection<Indicator> indicators,
        ICollection<int> updatedIndicatorIds,
        Dictionary<string, object>? formContext = null);
}

[RegisterScoped]
public class FormulaCalculator : IFormulaCalculator
{
    private static readonly Regex IndicatorReferenceRegex = new(@"indicator_(\d+)", RegexOptions.Compiled);

    // Поддержка вложенных ключей: context.product.cycleTime, context.cycleTime и т.д.
    private static readonly Regex ContextReferenceRegex = new(@"context\.([\w.]+)", RegexOptions.Compiled);
    private static readonly Regex TimeToMinutesFunctionRegex = new(@"timeToMinutes\(([^)]+)\)", RegexOptions.Compiled);

    public Dictionary<int, object> CalculateFormulas(
        Dictionary<int, object> currentValues,
        ICollection<Indicator> indicators,
        ICollection<int> updatedIndicatorIds,
        Dictionary<string, object>? formContext = null)
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

            var calculatedValue = EvaluateFormula(formulaToCalculate.Formula!, calculatedValues, formContext);
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

    private object? EvaluateFormula(string formula, Dictionary<int, object> currentValues,
        Dictionary<string, object>? formContext)
    {
        try
        {
            // Заменяем функции timeToMinutes
            var expression = TimeToMinutesFunctionRegex.Replace(formula, match =>
            {
                var timeRange = match.Groups[1].Value.Trim('"', '\'');
                var minutes = ParseTimeRangeToMinutes(timeRange);
                return minutes.ToString(CultureInfo.InvariantCulture);
            });

            // Заменяем ссылки на контекст на их значения
            expression = ContextReferenceRegex.Replace(expression, match =>
            {
                var contextKey = match.Groups[1].Value;
                var contextValue = GetContextValue(contextKey, formContext);
                return ConvertToNumericString(contextValue);
            });

            // Заменяем ссылки на индикаторы на их значения
            expression = IndicatorReferenceRegex.Replace(expression, match =>
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

    private object? GetContextValue(string contextKey, Dictionary<string, object>? formContext)
    {
        if (formContext == null)
        {
            return 0;
        }

        // Поддержка вложенных ключей через точку (например, product.cycleTime)
        var keys = contextKey.Split('.');
        object? currentValue = formContext;

        foreach (var key in keys)
        {
            if (currentValue is Dictionary<string, object> dict)
            {
                if (!dict.TryGetValue(key, out currentValue))
                {
                    return 0;
                }
            }
            else if (currentValue is JsonElement jsonElement)
            {
                // Поддержка camelCase имен свойств
                var propertyName = key;
                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    // Пробуем найти свойство (сначала точное совпадение, потом camelCase)
                    if (jsonElement.TryGetProperty(propertyName, out var property))
                    {
                        currentValue = property;
                    }
                    else
                    {
                        // Пробуем найти свойство с учетом camelCase
                        var camelCaseName = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
                        if (jsonElement.TryGetProperty(camelCaseName, out property))
                        {
                            currentValue = property;
                        }
                        else
                        {
                            // Пробуем найти свойство с учетом PascalCase
                            var pascalCaseName = char.ToUpperInvariant(propertyName[0]) + propertyName.Substring(1);
                            if (jsonElement.TryGetProperty(pascalCaseName, out property))
                            {
                                currentValue = property;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        // Извлекаем числовое значение из JsonElement, если необходимо
        if (currentValue is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.True => 1,
                JsonValueKind.False => 0,
                _ => 0
            };
        }

        // Если значение уже числовое, возвращаем его
        if (currentValue is int or long or double or decimal or float)
        {
            return currentValue;
        }

        return currentValue;
    }

    private int ParseTimeRangeToMinutes(string timeRange)
    {
        // Формат: "HH:mm-HH:mm" (например, "08:00-09:00")
        try
        {
            var parts = timeRange.Split('-');
            if (parts.Length != 2)
            {
                return 0;
            }

            if (TimeOnly.TryParse(parts[0], out var startTime) &&
                TimeOnly.TryParse(parts[1], out var endTime))
            {
                var startMinutes = startTime.Hour * 60 + startTime.Minute;
                var endMinutes = endTime.Hour * 60 + endTime.Minute;

                // Обработка случая, когда время переходит через полночь
                if (endMinutes < startMinutes)
                {
                    endMinutes += 24 * 60;
                }

                return endMinutes - startMinutes;
            }
        }
        catch
        {
            // Игнорируем ошибки парсинга
        }

        return 0;
    }
}