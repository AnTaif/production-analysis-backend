using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Implementation.Forms;
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

        // Конвертируем типизированный контекст в отдельные поля DTO
        ProductContextDto? productDto = null;
        ICollection<ProductContextDto>? productsDto = null;
        OperationContextDto? operationDto = null;

        foreach (var (key, context) in form.Context)
        {
            if (key.Equals("multiProduct", StringComparison.OrdinalIgnoreCase) &&
                context is MultiProductFormContext multiProductContext)
            {
                productsDto = multiProductContext.Products.Select(p => new ProductContextDto
                {
                    ProductId = p.ProductId,
                    CycleTime = p.CycleTime,
                    WorkstationCapacity = p.WorkstationCapacity,
                    DailyRate = p.DailyRate
                }).ToList();
            }
            else if (key.Equals("product", StringComparison.OrdinalIgnoreCase) &&
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

    public static Dictionary<string, FormContextBase> ExtractDomainContext(this CreateFormRequest request)
    {
        var domainContext = new Dictionary<string, FormContextBase>();

        var paType = PaTypeHelper.TryParse(request.PaTypeId);
        if (paType == null)
        {
            throw new NotSupportedException($"Unknown form type: {request.PaTypeId}");
        }

        switch (paType.Value)
        {
            case PaType.SingleProductWithCycleTime:
                domainContext["product"] = CreateSingleProductContextWithCycleTime(request.Product!);
                break;

            case PaType.SingleProductWithWorkstationCapacity:
                domainContext["product"] = CreateSingleProductContextWithWorkstationCapacity(request.Product!);
                break;

            case PaType.MultipleProductsWithCycleTime:
                domainContext["multiProduct"] = CreateMultipleProductsContextWithCycleTime(request.Products!);
                break;

            default:
                throw new NotSupportedException($"Unsupported form type: {paType.Value}");
        }

        return domainContext;
    }

    private static ProductFormContext CreateSingleProductContextWithCycleTime(ProductContextDto product)
    {
        return new ProductFormContext(
            product.ProductId,
            product.CycleTime,
            null,
            product.DailyRate);
    }

    private static ProductFormContext CreateSingleProductContextWithWorkstationCapacity(ProductContextDto product)
    {
        return new ProductFormContext(
            product.ProductId,
            null,
            product.WorkstationCapacity,
            product.DailyRate);
    }

    private static MultiProductFormContext CreateMultipleProductsContextWithCycleTime(
        ICollection<ProductContextDto> products)
    {
        var productInfos = products.Select(p => new ProductInfo(
            p.ProductId,
            p.CycleTime,
            null,
            p.DailyRate)).ToList();

        return new MultiProductFormContext(productInfos);
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