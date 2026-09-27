namespace Vitalis.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(int userId, string username, IReadOnlyList<string> roles, int? doctorId, int? patientId);

    // Not a JWT — a high-entropy random string. Only its SHA-256 hash is stored
    // (auth.refresh_tokens.token_hash), never the raw value.
    string GenerateRefreshToken();
}
