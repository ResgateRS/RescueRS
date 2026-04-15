using System.Security.Cryptography;
using System.Text;

namespace ResgateRS.Infrastructure.Database.Encryption;

public static class PhoneHashHelper
{
    public static string ComputeHash(string phone)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(phone));
        return Convert.ToHexStringLower(bytes);
    }
}
