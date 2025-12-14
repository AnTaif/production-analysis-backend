using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Repositories;

public interface ITemplatesRepository
{
    Task<Template?> GetLatestByPaTypeIdAsync(int paTypeId);
}