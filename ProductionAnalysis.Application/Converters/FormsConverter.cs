using System.Text.Json;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;
using static ProductionAnalysis.Application.Domain.Forms.FormStatus;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

namespace ProductionAnalysis.Application.Converters;

public static class FormsConverter
{
    public static FormShortDto ToShortDto(this Form form)
    {
        return new FormShortDto(
            form.Id,
            form.PaTypeId,
            ConvertToClientFormStatus(form.Status),
            form.CreationDate,
            form.UpdateDate
        );
    }

    public static FormDto ToDto(this Form form)
    {
        var template = ParseTemplateSnapshot(form.TemplateSnapshot);

        return new FormDto(
            form.Id,
            form.PaTypeId,
            ConvertToClientFormStatus(form.Status),
            form.CreationDate,
            form.UpdateDate,
            form.Context,
            new List<FormRowDto>(), // TODO: пока пустой список
            template
        );
    }

    public static SearchFormsFilter ToDomain(this SearchFormsFilterDto dto)
    {
        return new SearchFormsFilter
        {
            DepartmentId = dto.DepartmentId,
            Status = dto.Status.HasValue ? ConvertToDomainFormStatus(dto.Status.Value) : null,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    public static CreateForm ToDomain(this CreateFormRequest request, Guid creatorId)
    {
        return new CreateForm
        {
            PaTypeId = request.PaTypeId,
            Context = request.Context,
            CreatorId = creatorId
        };
    }

    private static FormTemplateDto ParseTemplateSnapshot(string templateSnapshot)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(templateSnapshot);
            var root = jsonDoc.RootElement;

            var contextFields = new List<FormFieldDto>();
            var tableColumns = new List<FormFieldDto>();

            // Парсим contextFields, если они есть
            if (root.TryGetProperty("contextFields", out var contextFieldsElement))
            {
                contextFields = ParseFormFields(contextFieldsElement);
            }

            // Парсим tableColumns, если они есть
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

    private static FormStatus ConvertToDomainFormStatus(Client.Models.Forms.FormStatus clientStatus)
    {
        return clientStatus switch
        {
            Client.Models.Forms.FormStatus.InProgress => InProgress,
            Client.Models.Forms.FormStatus.Completed => Finished,
            _ => throw new ArgumentOutOfRangeException(nameof(clientStatus), clientStatus, null)
        };
    }

    private static Client.Models.Forms.FormStatus ConvertToClientFormStatus(FormStatus domainStatus)
    {
        return domainStatus switch
        {
            InProgress => Client.Models.Forms.FormStatus.InProgress,
            Finished => Client.Models.Forms.FormStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(domainStatus), domainStatus, null)
        };
    }
}