using Core.Database;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Repositories;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class PaUnitOfWork(
    DbContext dbContext,
    IDictionariesRepository dictionariesRepository,
    IFormsRepository formsRepository,
    ITemplatesRepository templatesRepository,
    UserRepository userRepository
)
    : UnitOfWork(dbContext), IPaUnitOfWork
{
    public IDictionariesRepository Dictionaries { get; } = dictionariesRepository;
    public IFormsRepository Forms { get; } = formsRepository;
    public ITemplatesRepository Templates { get; } = templatesRepository;
    public IUserRepository Users { get; } = userRepository;
}