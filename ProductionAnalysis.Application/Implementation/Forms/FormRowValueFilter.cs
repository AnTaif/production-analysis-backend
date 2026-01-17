using System.Collections.Frozen;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowValueFilter
{
    ICollection<FormRowValueData> FilterUpdatableValues(
        Dictionary<int, object> requestValues,
        Template template);
}

[RegisterScoped]
public class FormRowValueFilter : IFormRowValueFilter
{
    private static readonly FrozenSet<string> UpdatableInputTypes = new HashSet<string>
    {
        FieldInputTypes.Manual,
        FieldInputTypes.Dictionary
    }.ToFrozenSet();

    public ICollection<FormRowValueData> FilterUpdatableValues(
        Dictionary<int, object> requestValues,
        Template template)
    {
        var indicatorsDict = template.Indicators
            .Where(i => i.Id > 0 && !string.IsNullOrEmpty(i.InputType))
            .ToDictionary(i => i.Id, i => i);

        var filteredValues = new List<FormRowValueData>();
        foreach (var (indicatorId, value) in requestValues)
        {
            if (!indicatorsDict.TryGetValue(indicatorId, out var indicator)
                || !UpdatableInputTypes.Contains(indicator.InputType))
            {
                continue;
            }

            var processedValue = value;
            if (indicator.ValueType == FieldValueTypes.Time && value is string stringValue)
            {
                if (TimeOnly.TryParse(stringValue, out var timeOnly) ||
                    TimeOnly.TryParseExact(stringValue, "HH:mm", out timeOnly) ||
                    TimeOnly.TryParseExact(stringValue, "HH:mm:ss", out timeOnly))
                {
                    processedValue = timeOnly;
                }
            }

            filteredValues.Add(new FormRowValueData
            {
                IndicatorId = indicatorId,
                Value = processedValue
            });
        }

        return filteredValues;
    }
}