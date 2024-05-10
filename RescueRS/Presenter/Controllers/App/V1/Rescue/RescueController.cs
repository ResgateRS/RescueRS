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

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListMyRescues")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<IEnumerable<RescueDTO>>> ListMyRescues() =>
        await this.mainService.ListMyRescues();

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListPendingRescues")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<IEnumerable<RescueDTO>>> ListPendingRescues(double? latitude, double? longitude) =>
        await this.mainService.ListPendingRescues(latitude, longitude);

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListPendingRescuesByProximity")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<IEnumerable<RescueDTO>>> ListPendingRescuesByProximity(double latitude, double longitude) =>
        await this.mainService.ListPendingRescuesByProximity(latitude, longitude);

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListCompletedRescues")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<IEnumerable<RescueDTO>>> ListCompletedRescues() =>
        await this.mainService.ListCompletedRescues();

    [HttpGet("Details")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<RescueDTO>> DetailRescue(Guid rescueId) =>
        await this.mainService.DetailRescue(rescueId);

    [HttpPost("Confirm")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<object>> ConfirmRescue(RescueGuidDTO dto) =>
        await this.mainService.ConfirmRescue(dto);

    [HttpPost("Cancel")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<object>> CancelRescue(RescueGuidDTO dto) =>
        await this.mainService.CancelRescue(dto);

    [HttpPost("Start")]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<object>> StartRescue(RescueGuidDTO dto) =>
        await this.mainService.StartRescue(dto);
}

