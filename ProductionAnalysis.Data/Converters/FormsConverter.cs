using System.Text.Json;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Data.Models.Forms;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

namespace ProductionAnalysis.Data.Converters;

public static class FormsConverter
{
    public static Form ToDomain(this FormDbo dbo)
    {
        var context = JsonSerializer.Deserialize<Dictionary<string, FormContextBase>>(dbo.Context,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new Dictionary<string, FormContextBase>();

        var rows = dbo.FormRows
            .OrderBy(r => r.Order)
            .Select(r => r.ToDomain())
            .ToList();

        var template = TemplateParser.ParseTemplateSnapshot(dbo.TemplateSnapshot, dbo.PaTypeId);

        Dictionary<int, object>? totalValues = null;
        if (!string.IsNullOrEmpty(dbo.TotalValues))
        {
            totalValues = JsonSerializer.Deserialize<Dictionary<int, object>>(dbo.TotalValues,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        return new Form(
            dbo.Id,
            dbo.PaTypeId,
            (FormStatus)dbo.Status,
            dbo.CreationDate,
            dbo.UpdateDate,
            context,
            template,
            rows,
            dbo.CreatorId,
            dbo.ShiftId,
            dbo.DepartmentId,
            totalValues);
    }

    public static FormRow ToDomain(this FormRowDbo dbo)
    {
        var values = new Dictionary<string, FormRowValue>();

        foreach (var valueDbo in dbo.Values)
        {
            var value = DeserializeValue(valueDbo.Value);
            if (value != null)
            {
                var cumulativeValue = valueDbo.CumulativeValue != null
                    ? DeserializeValue(valueDbo.CumulativeValue)
                    : null;

                values[valueDbo.IndicatorId.ToString()] = new FormRowValue(
                    value,
                    cumulativeValue);
            }
        }

        return new FormRow(
            dbo.Order,
            dbo.IsAdditionalOperation,
            dbo.AdditionalOperationId,
            values);
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