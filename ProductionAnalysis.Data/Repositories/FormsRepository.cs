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
            // Используем PostgreSQL JSONB оператор -> для извлечения значения
            // и сравниваем его с departmentIdValue
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

    public async Task<Form> CreateAsync(CreateForm createForm)
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
        await dbContext.SaveChangesAsync();

        return formDbo.ToDomain();
    }

    public async Task<Form?> GetByIdAsync(int formId)
    {
        var formDbo = await dbContext.Forms
            .FirstOrDefaultAsync(f => f.Id == formId);

        return formDbo?.ToDomain();
    }
}