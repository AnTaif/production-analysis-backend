using System.Text.Json;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

internal static class FormTemplateParser
{
    public static FormTemplateDto ParseTemplateSnapshot(string templateSnapshot)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(templateSnapshot);
            var root = jsonDoc.RootElement;

            var contextFields = new List<FormFieldDto>();
            var tableColumns = new List<FormFieldDto>();

            if (root.TryGetProperty("contextFields", out var contextFieldsElement))
            {
                contextFields = ParseFormFields(contextFieldsElement);
            }

            if (root.TryGetProperty("tableColumns", out var tableColumnsElement))
            {
                tableColumns = ParseFormFields(tableColumnsElement);
            }

            return new FormTemplateDto(contextFields, tableColumns);
        }
        catch
        {
            // Если не удалось распарсить, возвращаем пустой шаблон
            return new FormTemplateDto(new List<FormFieldDto>(), new List<FormFieldDto>());
        }
    }

    private static List<FormFieldDto> ParseFormFields(JsonElement fieldsElement)
    {
        var fields = new List<FormFieldDto>();

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
                    ? (FormFieldInputType)inputTypeElement.GetInt32()
                    : FormFieldInputType.Manual;

                var inputSelector = fieldElement.TryGetProperty("inputSelector", out var inputSelectorElement)
                    ? inputSelectorElement.GetString()
                    : null;

                var valueType = fieldElement.TryGetProperty("valueType", out var valueTypeElement)
                    ? valueTypeElement.GetString()
                    : null;

                fields.Add(new FormFieldDto(id, name, inputType, inputSelector, valueType));
            }
        }

        return fields;
    }
}