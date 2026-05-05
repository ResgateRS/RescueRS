using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResgateRS.Attributes;
using ResgateRS.Auth;
using ResgateRS.Domain.Application.Services;
using ResgateRS.DTOs;
using ResgateRS.Infrastructure.Database;
using ResgateRS.Middleware;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;

namespace ResgateRS.Presenter.Controllers.App.V1;

[ApiController]
[Route("api/v{version:apiVersion}/Rescue")]
[ApiVersion("1.0")]
public class RescueController(RescueService service, IServiceProvider serviceProvider) : BaseController<RescueService, IServiceProvider>(service, serviceProvider)
{
    [HttpGet("MyRescuerRequest")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> MyRescuerRequest()
    {
        var session = serviceProvider.GetRequiredService<UserSession>();
        var db = serviceProvider.GetRequiredService<ResgateRSDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == session.UserId);

        return Ok(new
        {
            requested = user?.RescuerRequested ?? false,
            description = user?.RescuerRequestDescription
        });
    }

    [HttpPost("RequestRescuerRole")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> RequestRescuerRole([FromBody] RescuerRoleRequestDTO dto)
    {
        var session = serviceProvider.GetRequiredService<UserSession>();
        var db = serviceProvider.GetRequiredService<ResgateRSDbContext>();

        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == session.UserId);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        if (user.Rescuer)
            return Conflict(new { message = "Você já é resgatista." });

        user.RescuerRequested = true;
        user.RescuerRequestDescription = SanitizeInput(dto.Description);
        await db.SaveChangesAsync();

        return Ok(new { message = "Solicitação enviada. Aguarde aprovação." });
    }

    private static string? SanitizeInput(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty).Trim();
    }

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
    public async Task<IActionResult> ListPendingRescues(double? latitude, double? longitude)
    {
        if (!serviceProvider.GetRequiredService<UserSession>().Rescuer)
            return StatusCode(403, new { message = "Acesso restrito a resgatistas." });
        return Ok(await this.mainService.ListPendingRescues(latitude, longitude));
    }

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListPendingRescuesByProximity")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ListPendingRescuesByProximity(double latitude, double longitude)
    {
        if (!serviceProvider.GetRequiredService<UserSession>().Rescuer)
            return StatusCode(403, new { message = "Acesso restrito a resgatistas." });
        return Ok(await this.mainService.ListPendingRescuesByProximity(latitude, longitude));
    }

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListCompletedRescues")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ListCompletedRescues()
    {
        if (!serviceProvider.GetRequiredService<UserSession>().Rescuer)
            return StatusCode(403, new { message = "Acesso restrito a resgatistas." });
        return Ok(await this.mainService.ListCompletedRescues());
    }

    [PaginatedRequest("Id do último resgate", PaginationType.Cursor, typeof(Guid))]
    [HttpGet("ListInProgressRescues")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ListInProgressRescues()
    {
        if (!serviceProvider.GetRequiredService<UserSession>().Rescuer)
            return StatusCode(403, new { message = "Acesso restrito a resgatistas." });
        return Ok(await this.mainService.ListInProgressRescues());
    }

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

