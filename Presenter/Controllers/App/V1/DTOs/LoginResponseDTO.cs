namespace ResgateRS.Presenter.Controllers.App.V1.DTOs;
public class LoginResponseDTO
{
    public string Token { get; set; } = null!;
    public bool Rescuer { get; set; }
}
