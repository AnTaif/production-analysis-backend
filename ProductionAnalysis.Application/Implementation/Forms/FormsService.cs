using Core.Results;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormsService
{
    Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter);
    Task<FormShortDto> CreateAsync(CreateFormRequest request);
    Task<FormDto> GetByIdAsync(Guid formId);
}

[RegisterScoped]
public class FormsService : IFormsService
{
    public Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter)
    {
        throw new NotImplementedException();
    }

    public Task<FormShortDto> CreateAsync(CreateFormRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<FormDto> GetByIdAsync(Guid formId)
    {
        throw new NotImplementedException();
    }
}