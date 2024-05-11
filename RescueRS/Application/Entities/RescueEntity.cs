using RescueRS.Application.Enums;

namespace ResgateRS.Domain.Application.Entities;

public class RescueEntity
{
    public Guid RescueId { get; set; }
    public DateTimeOffset RequestDateTime { get; set; }
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
    public Guid ConfirmedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public Guid RequestedBy { get; set; }
    public string? Description { get; set; }
    public string ContactPhone { get; set; } = null!;
    internal double GetDistance(double latitude, double longitude) =>
        Math.Sqrt(Math.Pow(Latitude - latitude, 2) + Math.Pow(Longitude - longitude, 2));
}