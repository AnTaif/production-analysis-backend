using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Converters;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class FormsRepository(PaDbContext dbContext) : IFormsRepository
{
    public Task<(ICollection<Form> Forms, int TotalCount)> SearchFormsAsync(SearchFormsFilter filter)
    {
        var query = dbContext.Forms.AsQueryable();

        if (filter.Status.HasValue)
        {
            query = query.Where(f => f.Status == (int)filter.Status.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(f => f.DepartmentId == filter.DepartmentId.Value);
        }

        var totalCountTask = query.CountAsync();

        var formsDboTask = query
            .OrderByDescending(f => f.UpdateDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        Task.WaitAll(totalCountTask, formsDboTask);

        var totalCount = totalCountTask.Result;
        var formsDbo = formsDboTask.Result;

        var forms = formsDbo.Select(f => f.ToDomain()).ToList();

        return Task.FromResult<(ICollection<Form> Forms, int TotalCount)>((forms, totalCount));
    }

    public async Task<Form> CreateAsync(Form newForm)
    {
        var now = DateTime.UtcNow;

        var contextJson = JsonSerializer.Serialize(newForm.Context, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var templateSnapshotJson = newForm.TemplateSnapshot.SerializeTemplateSnapshot();

        string? totalValuesJson = null;
        if (newForm.TotalValues != null && newForm.TotalValues.Count > 0)
        {
            totalValuesJson = JsonSerializer.Serialize(newForm.TotalValues, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        var formDbo = new FormDbo
        {
            PaTypeId = newForm.PaTypeId,
            Status = (int)FormStatus.InProgress,
            Context = contextJson,
            TemplateSnapshot = templateSnapshotJson,
            TotalValues = totalValuesJson,
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

    public async Task UpdateTotalValuesAsync(int formId, Dictionary<int, object> totalValues, Guid userId)
    {
        var formDbo = await dbContext.Forms.FirstOrDefaultAsync(f => f.Id == formId);
        if (formDbo == null)
        {
            return;
        }

        string? totalValuesJson = null;
        if (totalValues != null && totalValues.Count > 0)
        {
            totalValuesJson = JsonSerializer.Serialize(totalValues, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        formDbo.TotalValues = totalValuesJson;
        formDbo.LastEditorId = userId;
        formDbo.UpdateDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
    }
}