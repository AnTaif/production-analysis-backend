using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Converters;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class TemplatesRepository(PaDbContext dbContext) : ITemplatesRepository
{
    public async Task<Template?> FindLatestVerAsync(int paTypeId)
    {
        var templateDbo = await dbContext.Templates
            .Include(t => t.Indicators)
            .Where(t => t.PaTypeId == paTypeId)
            .OrderByDescending(t => t.Version)
            .FirstOrDefaultAsync();

        return templateDbo?.ToDomain();
    }
}