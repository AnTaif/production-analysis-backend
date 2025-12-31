namespace ProductionAnalysis.Client.Models.Auth;

public record LoginResponse
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}