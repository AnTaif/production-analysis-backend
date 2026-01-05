using System.Text.Json;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Data.Models.Forms;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;
using PaType = ProductionAnalysis.Application.Domain.Forms.PaType;

namespace ProductionAnalysis.Data.Converters;

public static class FormsConverter
{
    public static Form ToDomain(this FormDbo dbo)
    {
        // Десериализуем из DBO моделей (с атрибутами сериализации)
        var contextDbo = JsonSerializer.Deserialize<Dictionary<string, FormContextBaseDbo>>(dbo.Context,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new Dictionary<string, FormContextBaseDbo>();

        // Преобразуем DBO модели в доменные модели
        var context = ConvertContextToDomain(contextDbo);

        var rows = dbo.FormRows
            .OrderBy(r => r.Order)
            .Select(r => r.ToDomain())
            .ToList();

        var paType = Enum.IsDefined(typeof(PaType), dbo.PaTypeId)
            ? (PaType)dbo.PaTypeId
            : throw new InvalidOperationException($"Invalid PaTypeId: {dbo.PaTypeId}");

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
            paType,
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
            values,
            dbo.ProductId);
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

    private static Dictionary<string, FormContext> ConvertContextToDomain(
        Dictionary<string, FormContextBaseDbo> contextDbo)
    {
        var context = new Dictionary<string, FormContext>();

        foreach (var (key, contextValue) in contextDbo)
        {
            FormContext? domainContext = contextValue switch
            {
                ProductFormContextDbo productDbo => new ProductContext(
                    productDbo.ProductId,
                    productDbo.CycleTime,
                    productDbo.WorkstationCapacity,
                    productDbo.DailyRate),
                MultiProductFormContextDbo multiProductDbo => new MultiProductContext(
                    multiProductDbo.Products.Select(p => new ProductContext(
                        p.ProductId,
                        p.CycleTime,
                        p.WorkstationCapacity,
                        p.DailyRate)).ToList()),
                _ => null
            };

            if (domainContext != null)
            {
                context[key] = domainContext;
            }
        }

        return context;
    }

    public static string SerializeContextToJson(Dictionary<string, FormContext> domainContext)
    {
        // Преобразуем доменные модели в DBO модели для сериализации
        var contextDbo = new Dictionary<string, FormContextBaseDbo>();

        foreach (var (key, contextValue) in domainContext)
        {
            FormContextBaseDbo? dboContext = contextValue switch
            {
                ProductContext productContext => new ProductFormContextDbo(
                    productContext.ProductId,
                    productContext.CycleTime,
                    productContext.WorkstationCapacity,
                    productContext.DailyRate),
                MultiProductContext multiProductContext => new MultiProductFormContextDbo(
                    multiProductContext.Products.Select(p => new ProductFormContextDbo(
                        p.ProductId,
                        p.CycleTime,
                        p.WorkstationCapacity,
                        p.DailyRate)).ToList()),
                _ => null
            };

            if (dboContext != null)
            {
                contextDbo[key] = dboContext;
            }
        }

        return JsonSerializer.Serialize(contextDbo, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}