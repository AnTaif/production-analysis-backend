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

    public Form Create(CreateForm createForm)
    {
        var now = DateTime.UtcNow;

        var contextJson = JsonSerializer.Serialize(createForm.Context);

        var formDbo = new FormDbo
        {
            PaTypeId = createForm.PaTypeId,
            Status = (int)FormStatus.InProgress,
            Context = contextJson,
            TemplateSnapshot = createForm.TemplateSnapshot,
            CreationDate = now,
            UpdateDate = now,
            CreatorId = createForm.CreatorId,
            LastEditorId = createForm.CreatorId
        };

        dbContext.Forms.Add(formDbo);

        return formDbo.ToDomain();
    }

    public async Task<int> GetCreatedFormIdAsync(Form form)
    {
        var formDbo = dbContext.Forms
            .Local
            .OrderByDescending(f => f.CreationDate)
            .FirstOrDefault(f =>
                f.PaTypeId == form.PaTypeId &&
                f.CreatorId.ToString() == form.Context.GetValueOrDefault("creatorId")?.ToString() &&
                Math.Abs((f.CreationDate - form.CreationDate).TotalSeconds) < 5);

        if (formDbo != null && formDbo.Id > 0)
        {
            return formDbo.Id;
        }

        // Если не нашли в локальном контексте, ищем в БД
        var formFromDb = await dbContext.Forms
            .Where(f => f.PaTypeId == form.PaTypeId &&
                        Math.Abs((f.CreationDate - form.CreationDate).TotalSeconds) < 5)
            .OrderByDescending(f => f.CreationDate)
            .FirstOrDefaultAsync();

        if (formFromDb != null)
        {
            return formFromDb.Id;
        }

        throw new InvalidOperationException("Cannot determine form ID after creation");
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
        var formDbo = await dbContext.Forms.FirstOrDefaultAsync(f => f.Id == formId);

        if (formDbo == null)
            return;

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

            // Создаем отдельные записи для каждого значения
            foreach (var valueData in row.Values)
            {
                var valueJson = JsonSerializer.Serialize(valueData.Value);

                formRow.Values.Add(new FormRowValueDbo
                {
                    IndicatorId = valueData.IndicatorId,
                    Value = valueJson
                });
            }

            formRows.Add(formRow);
        }

        dbContext.FormRows.AddRange(formRows);
    }
}