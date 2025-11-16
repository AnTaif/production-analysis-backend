using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ProductionAnalysis.Data.Models;

public class UserDbo : IdentityUser<Guid>
{
    [MaxLength(255)]
    public string FirstName { get; set; } = null!;

    [MaxLength(255)]
    public string LastName { get; set; } = null!;

    [MaxLength(255)]
    public string? MiddleName { get; set; }
}