using Microsoft.AspNetCore.Mvc;
using ResgateRS.Attributes;
using ResgateRS.Domain.Application.Services;
using ResgateRS.DTOs;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;

namespace ResgateRS.Presenter.Controllers.App.V1;

[ApiController]
[Route("api/v{version:apiVersion}/Rescue")]
[ApiVersion("1.0")]
public class RescueController(RescueService service, IServiceProvider serviceProvider) : BaseController<RescueService, IServiceProvider>(service, serviceProvider)
{
    [HttpPost("Request")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<object>> RequestRescue(RescueRequestDTO dto) =>
        await this.mainService.RequestRescue(dto);

    [HttpPost("Confirm")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<object>> ConfirmRescue(RescueConfirmDTO dto) =>
        await this.mainService.ConfirmRescue(dto);

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListMyRescues")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<IEnumerable<RescueCardDTO>>> ListMyRescues() =>
        await this.mainService.ListMyRescues();

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListPendingRescues")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<IEnumerable<RescueCardDTO>>> ListPendingRescues() =>
        await this.mainService.ListPendingRescues();

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListPendingRescuesByProximity")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<IEnumerable<RescueCardDTO>>> ListPendingRescuesByProximity(double latitude, double longitude) =>
        await this.mainService.ListPendingRescuesByProximity(latitude, longitude);

    [HttpGet("Details")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<RescueDTO>> DetailRescue(Guid rescueId) =>
        await this.mainService.DetailRescue(rescueId);

    //     [HttpGet("Details")]
    // [MapToApiVersion("1.0")]
    // public async Task<RescueDTO>> GuidTest(Guid? rescueId) =>
    //     await this.mainService.GuidTest(rescueId);
}

