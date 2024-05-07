using Microsoft.AspNetCore.Mvc;
using ResgateRS.Attributes;
using ResgateRS.Domain.Application.Services;
using ResgateRS.DTOs;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;

namespace ResgateRS.Presenter.Controllers.App.V1;

[ApiController]
[Route("api/v{version:apiVersion}/Login")]
[ApiVersion("1.0")]
public class LoginController(LoginService service, IServiceProvider serviceProvider) : BaseController<LoginService, IServiceProvider>(service, serviceProvider)
{
    [SkipAuthentication]
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IResponse<LoginResponseDTO>> Login(LoginRequestDTO dto) =>
        await this.mainService.handle(dto);
}

