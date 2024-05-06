using ResgateRS.Domain.Application.Entities;

namespace ResgateRS.Presenter.Controllers.App.V1.DTOs;
public class RescueCardDTO
{
    public Guid RescueId { get; set; }
    public DateTimeOffset RequestDateTime { get; set; }
    public int TotalPeopleNumber { get; set; }
    public int ChildrenNumber { get; set; }
    public int ElderlyNumber { get; set; }
    public int DisabledNumber { get; set; }
    public int AnimalsNumber { get; set; }
    public bool Rescued { get; set; }

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
            Rescued = entity.Rescued
        };
}
