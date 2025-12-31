using System.Text.Json;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;
using ProductionAnalysis.Data.Models.Forms;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

namespace ProductionAnalysis.Data.Converters;

public static class FormsConverter
{
    public static Form ToDomain(this FormDbo dbo)
    {
        var context = JsonSerializer.Deserialize<Dictionary<string, object>>(dbo.Context)
                      ?? new Dictionary<string, object>();

        var rows = dbo.FormRows
            .OrderBy(r => r.Order)
            .Select(r => r.ToDomain())
            .ToList();

        return new Form
        {
            Id = dbo.Id,
            PaTypeId = dbo.PaTypeId,
            Status = (FormStatus)dbo.Status,
            CreationDate = dbo.CreationDate,
            UpdateDate = dbo.UpdateDate,
            Context = context,
            TemplateSnapshot = dbo.TemplateSnapshot,
            Rows = rows
        };
    }

    public static FormRow ToDomain(this FormRowDbo dbo)
    {
        var values = new Dictionary<string, object>();

        foreach (var valueDbo in dbo.Values)
        {
            var value = DeserializeValue(valueDbo.Value);
            if (value != null)
            {
                // Используем ID индикатора как ключ, но можно также использовать имя индикатора
                values[valueDbo.IndicatorId.ToString()] = value;
            }
        }

        return new FormRow
        {
            Order = dbo.Order,
            IsAdditionalOperation = dbo.IsAdditionalOperation,
            AdditionalOperationId = dbo.AdditionalOperationId,
            Values = values
        };
    }

    public static FormRowDto ToDto(this FormRowDbo dbo)
    {
        var values = new Dictionary<string, object>();

        foreach (var valueDbo in dbo.Values)
        {
            var value = DeserializeValue(valueDbo.Value);
            if (value != null)
            {
                // Используем имя индикатора как ключ для удобства на фронтенде
                var indicatorName = valueDbo.Indicator?.Name ?? valueDbo.IndicatorId.ToString();
                values[indicatorName] = value;
            }
        }

        return new FormRowDto
        {
            Order = dbo.Order,
            IsAdditionalOperation = dbo.IsAdditionalOperation,
            Values = values
        };
    }

    private static object? DeserializeValue(string jsonValue)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonValue);
            var root = doc.RootElement;

            return root.ValueKind switch
            {
                JsonValueKind.String => root.GetString(),
                JsonValueKind.Number => root.TryGetInt64(out var intVal)
                    ? intVal
                    : root.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => jsonValue
            };
        }
        catch
        {
            return jsonValue;
        }
    }
}