using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HC.LIS.Modules.UserAccess.Application.Users;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Authentication;

internal class JwtTokenService : IJwtTokenService
{
    private static readonly string SecretKey =
        Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_JWT_SECRET_KEY")
        ?? "hclis-dev-secret-key-for-development-only";

    private static readonly string Issuer =
        Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_JWT_ISSUER")
        ?? "hclis";

    private static readonly string Audience =
        Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_JWT_AUDIENCE")
        ?? "hclis-api";

    public string GenerateToken(Guid userId, string email, string role)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        string headerB64 = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            alg = "HS256",
            typ = "JWT"
        }));

        string payloadB64 = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = userId.ToString(),
            email,
            role,
            iss = Issuer,
            aud = Audience,
            iat = now.ToUnixTimeSeconds(),
            exp = now.AddHours(1).ToUnixTimeSeconds()
        }));

        string signingInput = $"{headerB64}.{payloadB64}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
        byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));

        return $"{signingInput}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
