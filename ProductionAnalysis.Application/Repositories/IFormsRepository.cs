using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Repositories;

public interface IFormsRepository
{
    Task<(ICollection<Form> Forms, int TotalCount)> SearchFormsAsync(SearchFormsFilter filter);
    Task<Form> CreateAsync(CreateForm createForm);
    Task<Form?> FindAsync(int formId);
    Task CreateFormRowsAsync(int formId, ICollection<FormRowData> rows);
}