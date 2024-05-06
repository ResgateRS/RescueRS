using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace ResgateRS.Tools;
public static class JwtManager
{
    public static string secret { get; set; } = "";
    public static double expirationMinutes { get; set; } = 30;

    private static SymmetricSecurityKey key
    {
        get
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }
    }

    public static string GenerateToken(IJwtPayload userData, DateTime? expirationTime = null)
    {
        expirationTime = expirationTime ?? DateTime.UtcNow.AddMinutes(expirationMinutes);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object>()
            {
                { "userData", userData }
            },
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
        catch
        {
            return false;
        }
        return true;
    }

    public static bool IsValidToken<T>(string? token, out T? userData) where T : IJwtPayload
    {
        userData = default(T);
        if (token.IsNullOrEmpty())
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
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
            if (tokenHandler.CanReadToken(token))
                userData = JsonSerializer.Deserialize<T>(tokenHandler.ReadJwtToken(token).Payload["userData"].ToString() ?? "");
        }
        catch
        {
            return false;
        }
        return true;
    }

    public static T? ExtractPayload<T>(string token) where T : IJwtPayload
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        if (tokenHandler.CanReadToken(token))
            return JsonSerializer.Deserialize<T>(tokenHandler.ReadJwtToken(token).Payload["userData"].ToString() ?? "");
        return default(T);
    }

    public class IJwtPayload { }
}