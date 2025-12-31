using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Repositories;

public interface ITemplatesRepository
{
    Task<Template?> FindLatestVerAsync(int paTypeId);
}