namespace ResgateRS.Domain.Application.Entities;

public class UserEntity
{
    public Guid UserId { get; set; }
    public bool Rescuer { get; set; }
    public bool Admin { get; set; }
    public bool Banned { get; set; }
    public bool RescuerRequested { get; set; }
    public string? RescuerRequestDescription { get; set; }
    public string Cellphone { get; set; } = null!;
    public string CellphoneHash { get; set; } = null!;
}