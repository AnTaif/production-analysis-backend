using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
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
        var template = ConvertTemplateToDto(form.TemplateSnapshot);

        var rows = form.Rows
            .OrderBy(r => r.Order)
            .Select(r => r.ToRowDto())
            .ToList();

        // Конвертируем типизированный контекст в отдельные поля DTO
        ProductContextDto? productDto = null;
        OperationContextDto? operationDto = null;

        foreach (var (key, context) in form.Context)
        {
            if (key.Equals("product", StringComparison.OrdinalIgnoreCase) &&
                context is ProductFormContext productContext)
            {
                productDto = new ProductContextDto
                {
                    ProductId = productContext.ProductId,
                    CycleTime = productContext.CycleTime,
                    WorkstationCapacity = productContext.WorkstationCapacity,
                    DailyRate = productContext.DailyRate
                };
            }
            // Можно добавить обработку для OperationContext в будущем
        }

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
                Operation = operationDto
            },
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
            Values = values
        };
    }

    public static Dictionary<string, FormContextBase> ExtractDomainContext(this CreateFormRequest request)
    {
        var domainContext = new Dictionary<string, FormContextBase>();

        if (request.Product != null)
        {
            domainContext["product"] = new ProductFormContext(
                request.Product.ProductId,
                request.Product.CycleTime,
                request.Product.WorkstationCapacity,
                request.Product.DailyRate);
        }

        // Можно добавить обработку для OperationContext в будущем
        if (request.Operation != null)
        {
            // TODO: создать OperationFormContext когда будет определен
        }

        return domainContext;
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