using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Repositories;

public interface IFormsRepository
{
    Task<(ICollection<Form> Forms, int TotalCount)> SearchFormsAsync(SearchFormsFilter filter);
    Task<Form> CreateAsync(Form newForm);
    Task<Form?> FindAsync(int formId);
    Task UpdateTotalValuesAsync(int formId, Dictionary<int, object> totalValues, Guid userId);
    Task UpdateStatusAsync(int formId, FormStatus status, Guid userId);
    Task DeleteAsync(int formId);
}