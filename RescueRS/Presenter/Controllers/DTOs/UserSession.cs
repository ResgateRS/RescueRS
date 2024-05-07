using static ResgateRS.Tools.JwtManager;

namespace ResgateRS.Auth;

public class UserSession : IJwtPayload
{   
    public Guid UserId { get; set; }
    public bool Rescuer { get; set; }
    public string Cellphone { get; set; } = string.Empty;
}