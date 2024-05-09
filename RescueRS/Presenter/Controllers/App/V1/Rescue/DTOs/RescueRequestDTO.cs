using ResgateRS.Domain.Application.Entities;

namespace ResgateRS.Presenter.Controllers.App.V1.DTOs;
public class RescueRequestDTO
{
    public int AdultsNumber { get; set; }
    public int ChildrenNumber { get; set; }
    public int ElderlyNumber { get; set; }
    public int DisabledNumber { get; set; }
    public int AnimalsNumber { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
