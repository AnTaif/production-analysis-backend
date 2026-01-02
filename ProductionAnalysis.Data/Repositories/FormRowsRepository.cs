using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Repositories;

public class FormRowsRepository(PaDbContext dbContext) : IFormRowsRepository
{
    public void AddRows(int formId, ICollection<FormRowData> rows)
    {
        var formRows = new List<FormRowDbo>();

        foreach (var row in rows)
        {
            var formRow = new FormRowDbo
            {
                FormId = formId,
                Order = row.Order,
                IsAdditionalOperation = row.IsAdditionalOperation,
                AdditionalOperationId = row.AdditionalOperationId,
                Values = new List<FormRowValueDbo>()
            };

            foreach (var valueData in row.Values)
            {
                var valueJson = JsonSerializer.Serialize(valueData.Value);

                formRow.Values.Add(new FormRowValueDbo
                {
                    FormId = formId,
                    FormRowOrder = row.Order,
                    IndicatorId = valueData.IndicatorId,
                    Value = valueJson
                });
            }

            formRows.Add(formRow);
        }

        dbContext.FormRows.AddRange(formRows);
    }

    public async Task UpdateRowValuesAsync(int formId, short rowOrder, ICollection<FormRowValueData> values,
        Guid userId)
    {
        var formRow = await dbContext.FormRows
            .Include(r => r.Values)
            .FirstOrDefaultAsync(r => r.FormId == formId && r.Order == rowOrder);

        if (formRow == null)
        {
            throw new InvalidOperationException($"Form row with FormId={formId} and Order={rowOrder} not found");
        }

        var now = DateTime.UtcNow;

        foreach (var valueData in values)
        {
            var valueDbo = formRow.Values.FirstOrDefault(v => v.IndicatorId == valueData.IndicatorId);

            if (valueDbo == null)
            {
                var valueJson = JsonSerializer.Serialize(valueData.Value);
                formRow.Values.Add(new FormRowValueDbo
                {
                    FormId = formId,
                    FormRowOrder = rowOrder,
                    IndicatorId = valueData.IndicatorId,
                    Value = valueJson
                });
            }
            else
            {
                var valueJson = JsonSerializer.Serialize(valueData.Value);
                valueDbo.Value = valueJson;
            }
        }

        var formDbo = await dbContext.Forms.FirstOrDefaultAsync(f => f.Id == formId);
        if (formDbo != null)
        {
            formDbo.UpdateDate = now;
            formDbo.LastEditorId = userId;
        }
    }
}