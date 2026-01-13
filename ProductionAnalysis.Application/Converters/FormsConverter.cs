using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

public static class FormsConverter
{
    public static FormShortDto ToShortDto(
        this Form form,
        EmployeeDto creator,
        EmployeeDto assignee,
        ShiftDto shift,
        Dictionary<int, string>? productsById = null,
        Dictionary<int, string>? operationsById = null)
    {
        var productNames = BuildProductNames(form, productsById, operationsById);

        return new FormShortDto
        {
            Id = form.Id,
            PaType = form.PaType.ToDto(),
            Status = FormStatusConverter.ConvertToClientFormStatus(form.Status),
            CreationDate = form.CreationDate,
            UpdateDate = form.UpdateDate,
            FormDate = form.FormDate,
            DepartmentId = form.DepartmentId,
            Creator = creator,
            Assignee = assignee,
            ProductNames = productNames,
            Shift = shift
        };
    }

    private static string BuildProductNames(
        Form form,
        Dictionary<int, string>? productsById,
        Dictionary<int, string>? operationsById)
    {
        if (productsById == null && operationsById == null)
            return string.Empty;

        // Для одного продукта
        var productContext = form.Context.GetProductContext();
        if (productContext != null)
        {
            if (productsById?.TryGetValue(productContext.ProductId, out var productName) == true)
                return productName;
            return string.Empty;
        }

        // Для нескольких продуктов
        var multiProductContext = form.Context.GetMultiProductContext();
        if (multiProductContext != null)
        {
            var productNames = multiProductContext.Products
                .Where(p => productsById?.TryGetValue(p.ProductId, out _) == true)
                .Select(p => productsById![p.ProductId])
                .ToList();

            return productNames.Count > 0 ? string.Join(", ", productNames) : string.Empty;
        }

        // Для операций или продуктов
        var operationOrProductContext = form.Context.GetOperationOrProductContext();
        if (operationOrProductContext != null)
        {
            if (operationOrProductContext.OperationId.HasValue)
            {
                if (operationsById?.TryGetValue(operationOrProductContext.OperationId.Value, out var operationName) ==
                    true)
                    return operationName;
            }
            else if (operationOrProductContext.ProductId.HasValue)
            {
                if (productsById?.TryGetValue(operationOrProductContext.ProductId.Value, out var productName) == true)
                    return productName;
            }
        }

        return string.Empty;
    }

    public static FormDto ToDto(this Form form)
    {
        var template = ConvertTemplateToDto(form.TemplateSnapshot, form.PaType);

        var rows = form.Rows
            .OrderBy(r => r.Order)
            .Select(r => r.ToRowDto())
            .ToList();

        var productDto = form.Context.GetProductContext()?.ToDto();
        var productsDto = form.Context.GetMultiProductContext()?.Products.Select(p => p.ToDto()).ToList();
        var operationOrProductDto = form.Context.GetOperationOrProductContext()?.ToDto();

        return new FormDto
        {
            Id = form.Id,
            PaType = form.PaType.ToDto(),
            Status = FormStatusConverter.ConvertToClientFormStatus(form.Status),
            CreationDate = form.CreationDate,
            UpdateDate = form.UpdateDate,
            FormDate = form.FormDate,
            Context = new FormContextDto
            {
                Product = productDto,
                Products = productsDto,
                OperationOrProduct = operationOrProductDto
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
                Value = rowValue.Value
            };
        }

        return new FormRowDto
        {
            Order = row.Order,
            IsAuxiliaryOperation = row.IsAuxiliaryOperation,
            ProductId = row.ProductId,
            GroupKey = row.GroupKey,
            Values = values
        };
    }

    private static FormTemplateDto ConvertTemplateToDto(Template template, PaType paType)
    {
        var mergedIndicatorIds = GetMergedIndicatorIds(paType, template);

        return new FormTemplateDto
        {
            TableColumns = template.Indicators
                .OrderBy(i => i.Order)
                .Select(indicator => new FormFieldDto
                {
                    Id = indicator.Id,
                    Name = indicator.Name,
                    InputType = indicator.InputType,
                    InputSelector = indicator.ValueSelector,
                    ValueType = indicator.ValueType,
                    ShouldMergeInGroup = mergedIndicatorIds.Contains(indicator.Id)
                })
                .ToList()
        };
    }

    private static HashSet<int> GetMergedIndicatorIds(PaType paType, Template template)
    {
        // Для форм типа "Менее 1 шт. в час" объединяем следующие колонки для строк группы
        if (paType == PaType.LessThanOnePerHour)
        {
            var mergedIds = new HashSet<int>
            {
                ShiftConstants.WorktimeIndicatorId,
                ShiftConstants.PlanIndicatorId
            };

            // Добавляем дополнительные индикаторы, если они есть в шаблоне
            var indicatorIds = template.Indicators.Select(i => i.Id).ToHashSet();

            if (indicatorIds.Contains(3)) // Факт, шт.
                mergedIds.Add(3);
            if (indicatorIds.Contains(4)) // Отклонение, шт.
                mergedIds.Add(4);
            if (indicatorIds.Contains(5)) // Простой, мин.
                mergedIds.Add(5);

            return mergedIds;
        }

        return [];
    }
}