using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Repositories;

public interface IFormsRepository
{
    Task<(ICollection<Form> Forms, int TotalCount)> SearchFormsAsync(SearchFormsFilter filter);
    Task<Form> CreateAsync(Form newForm);
    Task<Form?> FindAsync(int formId);
    Task CreateFormRowsAsync(int formId, ICollection<FormRowData> rows);
    Task UpdateFormRowValuesAsync(int formId, short rowOrder, ICollection<FormRowValueData> values, Guid userId);
}