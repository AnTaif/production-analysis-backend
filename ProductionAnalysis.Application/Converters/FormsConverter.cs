using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Implementation.Forms.Context;
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
            UpdateDate = form.UpdateDate,
            DepartmentId = form.DepartmentId,
        };
    }

    public static FormDto ToDto(this Form form)
    {
        var template = ConvertTemplateToDto(form.TemplateSnapshot);

        var rows = form.Rows
            .OrderBy(r => r.Order)
            .Select(r => r.ToRowDto())
            .ToList();

        var productDto = form.GetProductContext()?.ToDto();
        var productsDto = form.GetMultiProductContext()?.Products.Select(p => p.ToDto()).ToList();
        OperationContextDto? operationDto = null; // Можно добавить обработку для OperationContext в будущем

        return new FormDto
        {
            Id = form.Id,
            PaTypeId = form.PaTypeId,
            Status = FormStatusConverter.ConvertToClientFormStatus(form.Status),
            CreationDate = form.CreationDate,
            UpdateDate = form.UpdateDate,
            Context = new FormContextDto
            {
                Product = productDto,
                Products = productsDto,
                Operation = operationDto
            },
            Rows = rows,
            Template = template,
            TotalValues = form.TotalValues
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
            .Select(r => r.ToRowDto())
            .ToList();
    }

    public static FormRowDto ToRowDto(this FormRow row)
    {
        var values = new Dictionary<string, FormRowValueDto>();

        foreach (var (key, rowValue) in row.Values)
        {
            values[key] = new FormRowValueDto
            {
                Value = rowValue.Value,
                CumulativeValue = rowValue.CumulativeValue
            };
        }

        return new FormRowDto
        {
            Order = row.Order,
            IsAdditionalOperation = row.IsAdditionalOperation,
            ProductId = row.ProductId,
            Values = values
        };
    }

    [Obsolete("Use IFormContextFactory instead")]
    public static Dictionary<string, FormContext> ExtractDomainContext(this CreateFormRequest request)
    {
        // Для обратной совместимости используем фабрику напрямую
        var factory = new FormContextFactory();
        return factory.CreateContext(request);
    }

    private static FormTemplateDto ConvertTemplateToDto(Template template)
    {
        return new FormTemplateDto
        {
            TableColumns = template.Indicators
                .OrderBy(i => i.Id)
                .Select(indicator => new FormFieldDto
                {
                    Id = indicator.Id,
                    Name = indicator.Name,
                    InputType = indicator.InputType,
                    InputSelector = indicator.ValueSelector,
                    ValueType = indicator.ValueType,
                    IsCumulative = indicator.IsCumulative
                })
                .ToList()
        };
    }
}