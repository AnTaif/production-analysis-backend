using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Converters;

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
            query = query.Where(f =>
                EF.Functions.JsonContains(f.Context, $"{{\"department\": {departmentIdValue}}}"));
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
}