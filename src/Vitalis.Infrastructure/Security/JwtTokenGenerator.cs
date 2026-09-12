using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Vitalis.Application.Common;
using Vitalis.Application.Interfaces;

namespace Vitalis.Infrastructure.Security;

public class JwtTokenGenerator(JwtSettings settings) : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = settings;

    public string GenerateAccessToken(int userId, string username, IReadOnlyList<string> roles, int? doctorId, int? patientId)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            .. roles.Select(role => new Claim(ClaimTypes.Role, role)),
        ];

        if (doctorId is not null) claims.Add(new Claim("doctorId", doctorId.Value.ToString()));
        if (patientId is not null) claims.Add(new Claim("patientId", patientId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
