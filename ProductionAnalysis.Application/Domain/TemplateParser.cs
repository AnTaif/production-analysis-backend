using System.Text.Json;
using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Domain;

public static class TemplateParser
{
    public static Template ParseTemplateSnapshot(string templateSnapshot, int paTypeId)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(templateSnapshot);
            var root = jsonDoc.RootElement;

            var indicators = new List<Indicator>();

            if (root.TryGetProperty("tableColumns", out var tableColumnsElement))
            {
                indicators = ParseIndicators(tableColumnsElement);
            }

            return new Template
            {
                PaTypeId = paTypeId,
                Indicators = indicators
            };
        }
        catch
        {
            // Если не удалось распарсить, возвращаем пустой шаблон
            return new Template
            {
                PaTypeId = paTypeId,
                Indicators = new List<Indicator>()
            };
        }
    }

    private static List<Indicator> ParseIndicators(JsonElement fieldsElement)
    {
        var indicators = new List<Indicator>();

        if (fieldsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var fieldElement in fieldsElement.EnumerateArray())
            {
                var id = fieldElement.TryGetProperty("id", out var idElement)
                    ? idElement.GetInt32()
                    : 0;

                var name = fieldElement.TryGetProperty("name", out var nameElement)
                    ? nameElement.GetString() ?? string.Empty
                    : string.Empty;

                var inputType = fieldElement.TryGetProperty("inputType", out var inputTypeElement)
                    ? inputTypeElement.GetString() ?? string.Empty
                    : string.Empty;

                var inputSelector = fieldElement.TryGetProperty("inputSelector", out var inputSelectorElement)
                    ? inputSelectorElement.GetString()
                    : null;

                var valueType = fieldElement.TryGetProperty("valueType", out var valueTypeElement)
                    ? valueTypeElement.GetString() ?? string.Empty
                    : string.Empty;

                var formula = fieldElement.TryGetProperty("formula", out var formulaElement)
                    ? formulaElement.GetString()
                    : null;

                var isCumulative = fieldElement.TryGetProperty("isCumulative", out var isCumulativeElement)
                                   && isCumulativeElement.GetBoolean();

                var hasSummation = fieldElement.TryGetProperty("hasSummation", out var hasSummationElement)
                                   && hasSummationElement.GetBoolean();

                indicators.Add(new Indicator
                {
                    Id = id,
                    Name = name,
                    InputType = inputType,
                    ValueSelector = inputSelector,
                    ValueType = valueType,
                    Formula = formula,
                    IsCumulative = isCumulative,
                    HasSummation = hasSummation
                });
            }
        }

        return indicators;
    }
}