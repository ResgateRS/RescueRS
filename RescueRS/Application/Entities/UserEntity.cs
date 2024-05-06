namespace ResgateRS.Domain.Application.Entities;

public class UserEntity
{
    public Guid UserId { get; set; }
    public bool Rescuer { get; set; }
    public string Cellphone { get; set; } = null!;
}