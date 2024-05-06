namespace ResgateRS.Presenter.Controllers.App.V1.DTOs;
public class LoginRequestDTO
{
    public string Cellphone { get; set; } = null!;
    public bool Rescuer { get; set; }
}
