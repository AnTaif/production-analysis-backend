namespace ProductionAnalysis.Application.Domain;

public class User
{
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string? MiddleName { get; private set; }

    public string Email { get; private set; }

    public HashSet<string> Roles { get; private set; }

    public User(string firstName, string lastName, string? middleName, string email, HashSet<string> roles)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Email = email;
        Roles = roles;
    }
}