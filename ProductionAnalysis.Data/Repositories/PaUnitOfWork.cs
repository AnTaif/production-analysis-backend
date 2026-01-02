using Core.Database;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Context;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class PaUnitOfWork(
    PaDbContext dbContext,
    IDictionariesRepository dictionariesRepository,
    IFormsRepository formsRepository,
    IFormRowsRepository formRowsRepository,
    ITemplatesRepository templatesRepository,
    UserRepository userRepository
)
    : UnitOfWork(dbContext), IPaUnitOfWork
{
    public IDictionariesRepository Dictionaries { get; } = dictionariesRepository;
    public IFormsRepository Forms { get; } = formsRepository;
    public IFormRowsRepository FormRows { get; } = formRowsRepository;
    public ITemplatesRepository Templates { get; } = templatesRepository;
    public IUserRepository Users { get; } = userRepository;
}