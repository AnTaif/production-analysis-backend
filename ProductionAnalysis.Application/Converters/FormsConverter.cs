using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

public static class FormsConverter
{
    public static FormShortDto ToShortDto(this Form form)
    {
        return new FormShortDto(
            form.Id,
            form.PaTypeId,
            FormStatusConverter.ConvertToClientFormStatus(form.Status),
            form.CreationDate,
            form.UpdateDate
        );
    }

    public static FormDto ToDto(this Form form)
    {
        var template = FormTemplateParser.ParseTemplateSnapshot(form.TemplateSnapshot);

        var rows = form.Rows
            .OrderBy(r => r.Order)
            .Select(r => new FormRowDto(
                r.Order,
                r.IsAdditionalOperation,
                r.Values
            ))
            .ToList();

        return new FormDto(
            form.Id,
            form.PaTypeId,
            FormStatusConverter.ConvertToClientFormStatus(form.Status),
            form.CreationDate,
            form.UpdateDate,
            form.Context,
            rows,
            template
        );
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
}