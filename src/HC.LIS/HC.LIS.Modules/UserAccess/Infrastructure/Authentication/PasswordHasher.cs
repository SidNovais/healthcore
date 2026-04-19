using System.Security.Cryptography;
using System.Text;
using HC.LIS.Modules.UserAccess.Application.Users;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Authentication;

internal class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 310_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string plainText)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(plainText), salt, Iterations, Algorithm, KeySize);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool VerifyHashedPassword(string hashedPassword, string plainText)
    {
        string[] parts = hashedPassword.Split(':');
        if (parts.Length != 2) return false;
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);
        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(plainText), salt, Iterations, Algorithm, KeySize);
        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }
}
