using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace ResgateRS.Tools;

public static class JwtManager
{
    public static string secret { get; set; } = "";
    public static string appToken { get; set; } = "";
    public static double expiration { get; set; } = 5;
    private static SymmetricSecurityKey key
    {
        get
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }
    }

    public static string GenerateToken(Dictionary<string, object> userData, DateTime? expirationTime = null)
    {
        expirationTime = expirationTime ?? DateTime.UtcNow.AddMinutes(expiration);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = userData,
            Expires = expirationTime,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static bool IsValidToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            if (appToken.Equals(token))
                return true;
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public static T? ExtractPayload<T>(string token, string key)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        if (tokenHandler.CanReadToken(token))
        {
            try
            {
                return (T)tokenHandler.ReadJwtToken(token).Payload[key];
            }
            catch
            {
                return default;
            }
        }
        return default;
    }
}