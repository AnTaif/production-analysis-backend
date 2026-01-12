using System.Text.Json;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Domain;

public static class TemplateSerializer
{
    public static string SerializeTemplateSnapshot(this Template? template)
    {
        if (template == null)
        {
            return JsonSerializer.Serialize(new
            {
                id = 0,
                name = string.Empty,
                paTypeId = 0,
                version = 0,
                tableColumns = Array.Empty<object>()
            });
        }

        var tableColumns = template.Indicators
            .OrderBy(i => i.Order)
            .Select(indicator => new
            {
                id = indicator.Id,
                name = indicator.Name,
                inputType = indicator.InputType,
                inputSelector = indicator.ValueSelector,
                valueType = indicator.ValueType,
                formula = indicator.Formula,
                hasSummation = indicator.HasSummation,
                order = indicator.Order
            }).ToList();

        return JsonSerializer.Serialize(new
        {
            id = template.Id,
            name = template.Name,
            paTypeId = (int)template.PaType,
            version = template.Version,
            tableColumns
        });
    }
}