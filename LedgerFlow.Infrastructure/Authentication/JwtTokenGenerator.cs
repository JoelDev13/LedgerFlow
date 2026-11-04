using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LedgerFlow.Application.Interfaces.Authentication;
using LedgerFlow.Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LedgerFlow.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        string secretKey = _configuration["Jwt:Key"]
            ?? _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret key is not configured");

        string issuer = _configuration["Jwt:Issuer"] ?? "LedgerFlow";
        string audience = _configuration["Jwt:Audience"] ?? "LedgerFlow";
        int expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int exp) ? exp : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

