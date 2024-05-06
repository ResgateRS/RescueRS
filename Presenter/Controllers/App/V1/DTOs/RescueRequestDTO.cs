using ResgateRS.Domain.Application.Entities;

namespace ResgateRS.Presenter.Controllers.App.V1.DTOs;
public class RescueRequestDTO
{
    public DateTimeOffset? RequestDateTime { get; set; }
    public int TotalPeopleNumber { get; set; }
    public int ChildrenNumber { get; set; }
    public int ElderlyNumber { get; set; }
    public int DisabledNumber { get; set; }
    public int AnimalsNumber { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool Rescued { get; set; }
    public DateTimeOffset? RescueDateTime { get; set; }

    internal static RescueDTO FromEntity(RescueEntity entity) =>
        new()
        {
            RescueId = entity.RescueId,
            RequestDateTime = entity.RequestDateTime,
            TotalPeopleNumber = entity.TotalPeopleNumber,
            ChildrenNumber = entity.ChildrenNumber,
            ElderlyNumber = entity.ElderlyNumber,
            DisabledNumber = entity.DisabledNumber,
            AnimalsNumber = entity.AnimalsNumber,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Rescued = entity.Rescued,
            RescueDateTime = entity.RescueDateTime
        };
}
