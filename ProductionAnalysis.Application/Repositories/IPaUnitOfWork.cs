using Core.Database;

namespace ProductionAnalysis.Application.Repositories;

public interface IPaUnitOfWork : IUnitOfWork
{
    IDictionariesRepository Dictionaries { get; }
    IFormsRepository Forms { get; }
    ITemplatesRepository Templates { get; }
    IUserRepository Users { get; }
}