using System.Text.Json;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

public static class FormsConverter
{
    public static FormShortDto ToShortDto(this Form form)
    {
        return new FormShortDto
        {
            Id = form.Id,
            PaTypeId = form.PaTypeId,
            Status = FormStatusConverter.ConvertToClientFormStatus(form.Status),
            CreationDate = form.CreationDate,
            UpdateDate = form.UpdateDate
        };
    }

    public static FormDto ToDto(this Form form)
    {
        var template = FormTemplateParser.ParseTemplateSnapshot(form.TemplateSnapshot);

        var rows = form.Rows
            .OrderBy(r => r.Order)
            .Select(r => new FormRowDto
            {
                Order = r.Order,
                IsAdditionalOperation = r.IsAdditionalOperation,
                Values = r.Values
            })
            .ToList();

        return new FormDto
        {
            Id = form.Id,
            PaTypeId = form.PaTypeId,
            Status = FormStatusConverter.ConvertToClientFormStatus(form.Status),
            CreationDate = form.CreationDate,
            UpdateDate = form.UpdateDate,
            Context = form.Context,
            Rows = rows,
            Template = template
        };
    }

    public static SearchFormsFilter ToDomain(this SearchFormsFilterDto dto)
    {
        return new SearchFormsFilter
        {
            DepartmentId = dto.DepartmentId,
            Status = dto.Status.HasValue ? FormStatusConverter.ConvertToDomainFormStatus(dto.Status.Value) : null,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    public static List<FormRowDto> ToRowDtos(this ICollection<FormRow> rows)
    {
        return rows
            .OrderBy(r => r.Order)
            .Select(r => new FormRowDto
            {
                Order = r.Order,
                IsAdditionalOperation = r.IsAdditionalOperation,
                Values = r.Values
            })
            .ToList();
    }

    public static FormRowDto ToRowDto(this FormRow row)
    {
        return new FormRowDto
        {
            Order = row.Order,
            IsAdditionalOperation = row.IsAdditionalOperation,
            Values = row.Values
        };
    }

    public static Dictionary<string, object> ToDomainContext(
        this Dictionary<string, object> requestContext)
    {
        var domainContext = new Dictionary<string, object>();

        foreach (var (key, context) in requestContext)
        {
            // Сериализуем контекст в JSON, затем десериализуем в object
            // Это позволяет сохранить структуру данных
            var jsonString = JsonSerializer.Serialize(context, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);
            domainContext[key] = jsonElement;
        }

        return domainContext;
    }
}