using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Converters;
using ProductionAnalysis.Data.Models.Forms;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class FormsRepository(PaDbContext dbContext) : IFormsRepository
{
    public async Task<(ICollection<Form> Forms, int TotalCount)> SearchFormsAsync(SearchFormsFilter filter)
    {
        var query = dbContext.Forms.AsQueryable();

        if (filter.Status.HasValue)
        {
            query = query.Where(f => f.Status == (int)filter.Status.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            var departmentIdValue = filter.DepartmentId.Value;
            var departmentJson = $"{{\"department\": {departmentIdValue}}}";
            query = query.Where(f =>
                EF.Functions.JsonContains(f.Context, departmentJson));
        }

        var totalCount = await query.CountAsync();

        var formsDbo = await query
            .OrderByDescending(f => f.UpdateDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var forms = formsDbo.Select(f => f.ToDomain()).ToList();

        return (forms, totalCount);
    }

    public async Task<Form> CreateAsync(Form newForm)
    {
        var now = DateTime.UtcNow;

        var contextJson = JsonSerializer.Serialize(newForm.Context);

        var formDbo = new FormDbo
        {
            PaTypeId = newForm.PaTypeId,
            Status = (int)FormStatus.InProgress,
            Context = contextJson,
            TemplateSnapshot = newForm.TemplateSnapshot,
            CreationDate = now,
            UpdateDate = now,
            CreatorId = newForm.CreatorId,
            LastEditorId = newForm.CreatorId,
            ShiftId = newForm.ShiftId,
            DepartmentId = newForm.DepartmentId
        };

        dbContext.Forms.Add(formDbo);
        await dbContext.SaveChangesAsync();

        return formDbo.ToDomain();
    }

    public async Task<Form?> FindAsync(int formId)
    {
        var formDbo = await dbContext.Forms
            .Include(f => f.FormRows)
            .ThenInclude(r => r.Values)
            .ThenInclude(v => v.Indicator)
            .FirstOrDefaultAsync(f => f.Id == formId);

        return formDbo?.ToDomain();
    }

    public async Task CreateFormRowsAsync(int formId, ICollection<FormRowData> rows)
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

    public async Task UpdateFormRowValuesAsync(int formId, short rowOrder, ICollection<FormRowValueData> values,
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