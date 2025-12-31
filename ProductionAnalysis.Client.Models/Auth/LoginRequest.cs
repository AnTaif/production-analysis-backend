using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Auth;

public record LoginRequest
{
    [EmailAddress] public string Email { get; init; } = string.Empty;

    [MinLength(1)] public string Password { get; init; } = string.Empty;
}