using RescueRS.Application.Enums;
using ResgateRS.Domain.Application.Entities;

namespace ResgateRS.Presenter.Controllers.App.V1.DTOs;
public class RescueDTO
{
    public Guid RescueId { get; set; }
    public DateTimeOffset? RequestDateTime { get; set; }
    public int AdultsNumber { get; set; }
    public int ChildrenNumber { get; set; }
    public int ElderlyNumber { get; set; }
    public int DisabledNumber { get; set; }
    public int AnimalsNumber { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public RescueStatusEnum Status { get; set; }
    public DateTimeOffset? RescueDateTime { get; set; }
    public DateTimeOffset? UpdateDateTime { get; set; }
    public string Cellphone { get; set; } = null!;
    public double? Distance { get; set; }

    internal static RescueDTO FromEntity(RescueEntity entity, double? latitude = null, double? longitude = null) =>
        new()
        {
            RescueId = entity.RescueId,
            RequestDateTime = entity.RequestDateTime,
            AdultsNumber = entity.AdultsNumber,
            ChildrenNumber = entity.ChildrenNumber,
            ElderlyNumber = entity.ElderlyNumber,
            DisabledNumber = entity.DisabledNumber,
            AnimalsNumber = entity.AnimalsNumber,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Status = entity.Status,
            RescueDateTime = entity.RescueDateTime,
            UpdateDateTime = entity.UpdateDateTime,
            Cellphone = entity.ContactPhone,
            Distance = latitude != null && longitude != null ? GetDistanceInMeters(latitude.Value, longitude.Value, entity.Latitude, entity.Longitude) : null,
        };

    private const double EarthRadiusKm = 6371.0;

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    public static double GetDistanceInMeters(double latitude, double longitude, double entityLatitude, double entityLongitude)
    {
        if (latitude == entityLatitude && longitude == entityLongitude)
            return 0;

        double lat1 = ToRadians(latitude);
        double lon1 = ToRadians(longitude);
        double lat2 = ToRadians(entityLatitude);
        double lon2 = ToRadians(entityLongitude);

        double deltaLat = lat2 - lat1;
        double deltaLon = lon2 - lon1;

        double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2), 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c * 1000;
    }
}
