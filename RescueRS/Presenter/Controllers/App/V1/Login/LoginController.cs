using Microsoft.AspNetCore.Mvc;
using ResgateRS.Attributes;
using ResgateRS.Auth;
using ResgateRS.Domain.Application.Services;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;

namespace ResgateRS.Presenter.Controllers.App.V1;

[ApiController]
[Route("api/v{version:apiVersion}/Login")]
[ApiVersion("1.0")]
public class LoginController(LoginService service, UserSession userSession, IWebHostEnvironment env) : ControllerBase
{
    [SkipAuthentication]
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Login(LoginRequestDTO dto)
    {
        var result = await service.Handle(dto);

        if (result.OtpSent)
            return Ok(new { otpSent = true, message = "Código de verificação enviado para seu celular." });

        var isProduction = !env.IsEnvironment("Test");
        Response.Cookies.Append("auth_token", result.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(30)
        });

        return Ok(new { token = result.Token, rescuer = result.Rescuer, admin = result.Admin });
    }

    [HttpGet("Session")]
    [MapToApiVersion("1.0")]
    public IActionResult Session()
    {
        return Ok(new
        {
            token = Request.Cookies["auth_token"] ?? "",
            rescuer = userSession.Rescuer,
            admin = userSession.Admin,
        });
    }
}
