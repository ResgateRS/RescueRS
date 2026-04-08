using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResgateRS.Auth;
using ResgateRS.DTOs;
using ResgateRS.Infrastructure.Database;
using ResgateRS.Middleware;

namespace ResgateRS.Presenter.Controllers.App.V1;

[ApiController]
[Route("api/v{version:apiVersion}/Admin")]
[ApiVersion("1.0")]
public class AdminController(ResgateRSDbContext db, UserSession userSession) : ControllerBase
{
    [HttpGet("Users")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ListUsers()
    {
        if (!userSession.Admin)
            return Unauthorized(new { Message = "Acesso restrito a administradores." });

        var users = await db.Users.Select(u => new
        {
            u.UserId,
            u.Cellphone,
            u.Rescuer,
            u.Admin,
            u.Banned
        }).ToListAsync();

        return Ok(Response<object>.Success(users));
    }

    [HttpPost("Ban")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> BanUser([FromBody] BanRequestDTO dto)
    {
        if (!userSession.Admin)
            return Unauthorized(new { Message = "Acesso restrito a administradores." });

        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        user.Banned = dto.Banned;
        await db.SaveChangesAsync();

        return Ok(Response<object>.Success(null, dto.Banned ? "Usuário banido." : "Ban removido."));
    }

    [HttpPost("SetRole")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> SetRole([FromBody] SetRoleDTO dto)
    {
        if (!userSession.Admin)
            return Unauthorized(new { Message = "Acesso restrito a administradores." });

        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        user.Rescuer = dto.Rescuer;
        user.Admin = dto.Admin;
        await db.SaveChangesAsync();

        return Ok(Response<object>.Success(null, "Roles atualizadas."));
    }
}

public class BanRequestDTO
{
    public Guid UserId { get; set; }
    public bool Banned { get; set; }
}

public class SetRoleDTO
{
    public Guid UserId { get; set; }
    public bool Rescuer { get; set; }
    public bool Admin { get; set; }
}
