using ProductionAnalysis.Application.Domain.Common.ValueTypes;

namespace ProductionAnalysis.Application.Domain;

public class User
{
    public User(Guid id, string firstName, string lastName, string? middleName, string email, ICollection<string> roles)
    {
        Id = id;
        FullName = new FullName(lastName, firstName, middleName);
        Email = email;
        Roles = roles;
    }

    public Guid Id { get; }
    public FullName FullName { get; }
    public string Email { get; }
    public ICollection<string> Roles { get; }
}