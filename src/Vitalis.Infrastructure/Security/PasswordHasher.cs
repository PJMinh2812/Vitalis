using System.Security.Cryptography;
using Vitalis.Application.Interfaces;

namespace Vitalis.Infrastructure.Security;

// PBKDF2-HMACSHA256 via the .NET BCL — no extra package needed. Stored as
// "{iterations}.{salt-base64}.{hash-base64}" so the work factor can change
// later without invalidating hashes already in the database.
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expectedHash.Length);

        // Constant-time comparison — a plain `==` would leak how many leading
        // bytes matched through response timing, letting an attacker guess the
        // hash byte-by-byte.
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
