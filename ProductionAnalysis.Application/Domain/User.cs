namespace ProductionAnalysis.Application.Domain;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? MiddleName { get; set; }
    public string Email { get; set; }
    public ICollection<string> Roles { get; set; } = new List<string>();
}