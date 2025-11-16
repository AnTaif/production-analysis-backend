namespace ProductionAnalysis.Client.Models.Auth;

public record LoginResponse(
    string Email,
    string Token
);