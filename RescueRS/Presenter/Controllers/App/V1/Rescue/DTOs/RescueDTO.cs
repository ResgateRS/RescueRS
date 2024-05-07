using ResgateRS.Domain.Application.Entities;

namespace ResgateRS.Presenter.Controllers.App.V1.DTOs;
public class RescueDTO
{
    public Guid RescueId { get; set; }
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
    public string Cellphone { get; set; } = null!;
    public double? distance { get; set; }

    internal static RescueDTO FromEntity(RescueEntity entity, double? latitude = null, double? longitude = null) =>
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
            RescueDateTime = entity.RescueDateTime,
            distance = latitude != null && longitude != null ? GetDistance(latitude.Value, longitude.Value, entity.Latitude, entity.Longitude) : null,
        };

    internal static double GetDistance(double latitude, double longitude, double entityLatitude, double entityLongitude) =>
        Math.Sqrt(Math.Pow(entityLatitude - latitude, 2) + Math.Pow(entityLongitude - longitude, 2));
}
