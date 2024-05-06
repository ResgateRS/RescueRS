using static ResgateRS.Tools.JwtManager;

namespace ResgateRS.Auth;

public class UserSession : IJwtPayload
{   
    public string UserId { get; set; } = string.Empty;
    public bool Rescuer { get; set; }
    public string Cellphone { get; set; } = string.Empty;
}