using static ResgateRS.Tools.JwtManager;

namespace ResgateRS.Auth;

public class UserSession : IJwtPayload
{   
    public string UserId { get; set; } = null!;
    public bool Rescuer { get; set; }
    public string Cellphone { get; set; } = null!;
}