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
    public ICollection<FormRowValueData> FilterUpdatableValues(
        Dictionary<int, object> requestValues,
        Template template)
    {
        var indicatorsDict = template.Indicators
            .Where(i => i.Id > 0 && !string.IsNullOrEmpty(i.InputType))
            .ToDictionary(i => i.Id, i => i.InputType);

        var filteredValues = new List<FormRowValueData>();
        foreach (var (indicatorId, value) in requestValues)
        {
            if (!indicatorsDict.TryGetValue(indicatorId, out var inputType))
            {
                continue;
            }

            if (inputType is FieldInputTypes.Manual or FieldInputTypes.Dictionary)
            {
                filteredValues.Add(new FormRowValueData
                {
                    IndicatorId = indicatorId,
                    Value = value
                });
            }
        }

        return filteredValues;
    }
}