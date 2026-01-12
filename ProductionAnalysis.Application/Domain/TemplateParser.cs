using System.Text.Json;
using ProductionAnalysis.Application.Domain.Forms;
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

            var id = root.TryGetProperty("id", out var idElement)
                ? idElement.GetInt32()
                : 0;

            var name = root.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString() ?? string.Empty
                : string.Empty;

            var version = root.TryGetProperty("version", out var versionElement)
                ? versionElement.GetInt32()
                : 0;

            // Если paTypeId не указан в JSON, используем переданный параметр
            var templatePaTypeId = root.TryGetProperty("paTypeId", out var paTypeIdElement)
                ? paTypeIdElement.GetInt32()
                : paTypeId;

            var paType = Enum.IsDefined(typeof(PaType), templatePaTypeId)
                ? (PaType)templatePaTypeId
                : throw new InvalidOperationException($"Invalid PaTypeId: {templatePaTypeId}");

            var indicators = new List<Indicator>();

            if (root.TryGetProperty("tableColumns", out var tableColumnsElement))
            {
                indicators = ParseIndicators(tableColumnsElement);
            }

            return new Template(
                id,
                name,
                paType,
                version,
                indicators);
        }
        catch
        {
            var paType = Enum.IsDefined(typeof(PaType), paTypeId)
                ? (PaType)paTypeId
                : throw new InvalidOperationException($"Invalid PaTypeId: {paTypeId}");

            return new Template(
                0,
                string.Empty,
                paType,
                0,
                new List<Indicator>());
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

                var hasSummation = fieldElement.TryGetProperty("hasSummation", out var hasSummationElement)
                                   && hasSummationElement.GetBoolean();

                var order = fieldElement.TryGetProperty("order", out var orderElement)
                    ? orderElement.GetInt32()
                    : indicators.Count;

                indicators.Add(new Indicator(
                    id,
                    name,
                    valueType,
                    inputType,
                    inputSelector,
                    formula,
                    hasSummation,
                    order
                ));
            }
        }

        return indicators;
    }
}