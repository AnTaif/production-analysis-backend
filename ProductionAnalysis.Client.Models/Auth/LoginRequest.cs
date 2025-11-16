using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Auth;

public record LoginRequest(
    [EmailAddress] string Email,
    string Password
);