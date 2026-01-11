using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

public static class FormsConverter
{
    public static FormShortDto ToShortDto(this Form form, EmployeeDto creator, EmployeeDto executor)
    {
        return new FormShortDto
        {
            Id = form.Id,
            PaType = form.PaType.ToDto(),
            Status = FormStatusConverter.ConvertToClientFormStatus(form.Status),
            CreationDate = form.CreationDate,
            UpdateDate = form.UpdateDate,
            DepartmentId = form.DepartmentId,
            Creator = creator,
            Executor = executor,
        };
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
        var operationDto = form.Context.GetOperationContext()?.ToDto();

        return new FormDto
        {
            Id = form.Id,
            PaType = form.PaType.ToDto(),
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
                .OrderBy(i => i.Id)
                .Select(indicator => new FormFieldDto
                {
                    Id = indicator.Id,
                    Name = indicator.Name,
                    InputType = indicator.InputType,
                    InputSelector = indicator.ValueSelector,
                    ValueType = indicator.ValueType,
                    IsCumulative = indicator.IsCumulative,
                    ShouldMergeInGroup = mergedIndicatorIds.Contains(indicator.Id)
                })
                .ToList()
        };
    }

    private static HashSet<int> GetMergedIndicatorIds(PaType paType, Template template)
    {
        // Для форм типа "Менее 1 шт. в час" объединяем следующие колонки для строк группы:
        // - Время работы (16)
        // - План, шт. (1)
        // - Факт, шт. (2) - если есть в шаблоне
        // - Отклонение, шт. (3) - если есть в шаблоне
        // - Простой, мин. (4) - если есть в шаблоне
        if (paType == PaType.LessThanOnePerHour)
        {
            var mergedIds = new HashSet<int>
            {
                ShiftConstants.WorktimeIndicatorId, // 16 - Время работы
                ShiftConstants.PlanIndicatorId // 1 - План, шт.
            };

            // Добавляем дополнительные индикаторы, если они есть в шаблоне
            var indicatorIds = template.Indicators.Select(i => i.Id).ToHashSet();

            if (indicatorIds.Contains(2)) // Факт, шт.
                mergedIds.Add(2);
            if (indicatorIds.Contains(3)) // Отклонение, шт.
                mergedIds.Add(3);
            if (indicatorIds.Contains(4)) // Простой, мин.
                mergedIds.Add(4);

            return mergedIds;
        }

        return new HashSet<int>();
    }
}