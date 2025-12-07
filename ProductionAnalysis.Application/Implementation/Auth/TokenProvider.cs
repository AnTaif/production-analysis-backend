using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProductionAnalysis.Application.Domain;

namespace ProductionAnalysis.Application.Implementation.Auth;

public interface ITokenProvider
{
    string GenerateToken(User user);
}

[RegisterScoped]
public class JwtTokenProvider(IOptions<JwtOptions> options) : ITokenProvider
{
    private readonly JwtOptions options = options.Value;

    public string GenerateToken(User user)
    {
        var claims = CreateClaims(user);
        var signingCredentials = CreateSigningCredentials();
        var token = CreateJwtToken(claims, signingCredentials);

        var jwtTokenHandler = new JwtSecurityTokenHandler();
        return jwtTokenHandler.WriteToken(token);
    }

    private JwtSecurityToken CreateJwtToken(
        List<Claim> claims,
        SigningCredentials signingCredentials)
    {
        return new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(options.ExpiryMinutes),
            signingCredentials: signingCredentials);
    }

    private SigningCredentials CreateSigningCredentials()
    {
        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(options.Secret)),
            SecurityAlgorithms.HmacSha256);
    }

    private static List<Claim> CreateClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.MiddleName, user.MiddleName ?? ""),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Sid, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Email),
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }
}