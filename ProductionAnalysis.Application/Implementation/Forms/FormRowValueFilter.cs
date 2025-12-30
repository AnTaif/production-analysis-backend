using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowValueFilter
{
    ICollection<FormRowValueData> FilterUpdatableValues(
        Dictionary<int, object> requestValues,
        string templateSnapshot);
}

[RegisterScoped]
public class FormRowValueFilter : IFormRowValueFilter
{
    public ICollection<FormRowValueData> FilterUpdatableValues(
        Dictionary<int, object> requestValues,
        string templateSnapshot)
    {
        var templateSnapshotDto = FormTemplateParser.ParseTemplateSnapshot(templateSnapshot);
        var indicatorsDict = templateSnapshotDto.TableColumns
            .Where(c => c.Id > 0 && !string.IsNullOrEmpty(c.InputType))
            .ToDictionary(c => c.Id, c => c.InputType);

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