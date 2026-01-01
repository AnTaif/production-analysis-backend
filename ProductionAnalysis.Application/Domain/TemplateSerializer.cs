using System.Text.Json;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Domain;

public static class TemplateSerializer
{
    public static string SerializeTemplateSnapshot(this Template? template)
    {
        if (template == null || template.Indicators.Count == 0)
        {
            return JsonSerializer.Serialize(new
            {
                tableColumns = Array.Empty<object>()
            });
        }

        var tableColumns = template.Indicators
            .OrderBy(i => i.Id)
            .Select(indicator => new
            {
                id = indicator.Id,
                name = indicator.Name,
                inputType = indicator.InputType,
                inputSelector = indicator.ValueSelector,
                valueType = indicator.ValueType,
                formula = indicator.Formula
            }).ToList();

        return JsonSerializer.Serialize(new
        {
            tableColumns
        });
    }
}