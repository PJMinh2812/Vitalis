using System.Security.Cryptography;
using System.Text;

namespace Vitalis.Application.Common;

// SHA-256 is deliberate and different from IPasswordHasher: refresh tokens are
// already 64 bytes of random data (not a user-chosen password), so a slow,
// salted KDF buys nothing here — a plain hash is enough to look one up by
// exact match without ever storing the raw token (matches CHAR(64) hex in db.sql).
public static class TokenHasher
{
    public static string Sha256(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
}
