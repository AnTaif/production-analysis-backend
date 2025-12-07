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