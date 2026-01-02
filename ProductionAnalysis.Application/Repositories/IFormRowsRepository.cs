using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Repositories;

public interface IFormRowsRepository
{
    void AddRows(int formId, ICollection<FormRowData> rows);
    Task UpdateRowValuesAsync(int formId, short rowOrder, ICollection<FormRowValueData> values, Guid userId);

    Task UpdateMultipleRowsValuesAsync(int formId, Dictionary<short, ICollection<FormRowValueData>> rowsValues,
        Guid userId);
}