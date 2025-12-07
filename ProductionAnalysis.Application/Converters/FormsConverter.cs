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

        return new FormDto(
            form.Id,
            form.PaTypeId,
            FormStatusConverter.ConvertToClientFormStatus(form.Status),
            form.CreationDate,
            form.UpdateDate,
            form.Context,
            new List<FormRowDto>(), // Пока пустой список, будет заполняться при реализации функционала строк
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
            Context = request.Context,
            CreatorId = creatorId
        };
    }
}