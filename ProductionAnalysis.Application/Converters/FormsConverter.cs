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

    public static CreateForm ToDomain(this CreateFormRequest request, Guid creatorId)
    {
        return new CreateForm
        {
            PaTypeId = request.PaTypeId,
            ShiftId = request.ShiftId,
            Context = request.Context,
            CreatorId = creatorId
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
}