using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace ResgateRS.Tools;

public class JwtTool
{
    private string token;
    private string secret;
    private ClaimsIdentity subject;
    private DateTime createAt;
    private DateTime expires;
    private DateTime notBefore;

    public JwtTool()
    {
        this.token = "";
        this.secret = "";
        this.subject = new ClaimsIdentity();

        long createAtTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long notBeforeTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long expiresTimestamp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
        this.setCreateAt(createAtTimestamp);
        this.setNotBefore(notBeforeTimestamp);
        this.setExpires(expiresTimestamp);
    }

    public JwtTool(string token)
    {
        this.token = token;
        this.secret = "";
        this.subject = new ClaimsIdentity();
        
        long createAtTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long notBeforeTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long expiresTimestamp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
        this.setCreateAt(createAtTimestamp);
        this.setNotBefore(notBeforeTimestamp);
        this.setExpires(expiresTimestamp);
    }

    public void setSecret(string secret)
    {
        this.secret = secret;
    }

    public void setCreateAt(long timestamp)
    {
        DateTime datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        datetime = datetime.AddSeconds(timestamp).ToLocalTime();
        this.createAt = datetime;
    }

    public void setNotBefore(long timestamp)
    {
        DateTime datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        datetime = datetime.AddSeconds(timestamp).ToLocalTime();
        this.notBefore = datetime;
    }

    public void setExpires(long timestamp)
    {
        DateTime datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        datetime = datetime.AddSeconds(timestamp).ToLocalTime();
        this.expires = datetime;
    }

    public string generateToken()
    {
        var a = this.createAt.ToString();
        var b = this.notBefore.ToString();
        var c = this.expires.ToString();

        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(this.secret);
        var tokenDescriptor = new SecurityTokenDescriptor();
        tokenDescriptor.IssuedAt = this.createAt;
        tokenDescriptor.NotBefore = this.notBefore;
        tokenDescriptor.Expires = this.expires;
        tokenDescriptor.Subject = this.subject;
        tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool validateToken()
    {
        try
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(this.secret));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidation = new TokenValidationParameters();
            tokenValidation.ValidateIssuerSigningKey = true;
            tokenValidation.IssuerSigningKey = new SymmetricSecurityKey(hmac.Key);
            tokenValidation.ValidateLifetime = true;
            tokenValidation.ValidateAudience = false;
            tokenValidation.ValidateIssuer = false;
            tokenValidation.RequireExpirationTime = true;
            tokenValidation.ClockSkew = TimeSpan.Zero;

            tokenHandler.ValidateToken(this.token, tokenValidation, out SecurityToken validatedToken);
        }
        catch(SecurityTokenInvalidLifetimeException)
        {
            throw new Exception("Token is expired");
        }
        catch(SecurityTokenExpiredException)
        {
            throw new Exception("Token is expired");
        }
        catch(SecurityTokenInvalidSignatureException)
        {
            throw new Exception("Token is invalid");
        }
        catch(Exception)
        {
            throw new Exception("Token is invalid");
        }
        return true;
    }

    public void addContent(string key, string value)
    {
        this.subject.AddClaim(new Claim(key, value));
    }

    public void setUserData<T>(T value)
    {
        var strvalue = JsonSerializer.Serialize(value);
        this.subject.AddClaim(new Claim("userData", strvalue));
    }

    public string? getContent(string key)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);
        JwtPayload payload = jwtToken.Payload;
        foreach(Claim claim in payload.Claims)
        {
            if (claim.Type == key) {
                return claim.Value;
            }
        }
        return null;
    }
}
