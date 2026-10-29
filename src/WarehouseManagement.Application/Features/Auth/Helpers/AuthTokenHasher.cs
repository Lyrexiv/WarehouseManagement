using System.Security.Cryptography;
using System.Text;

namespace WarehouseManagement.Application.Features.Auth.Helpers;

internal static class AuthTokenHasher
{
    public static string Hash(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(
                "Token cannot be empty.",
                nameof(token));
        }

        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(bytes);
    }

    public static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes);
    }
}