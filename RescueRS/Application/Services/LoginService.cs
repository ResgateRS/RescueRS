using Microsoft.AspNetCore.Mvc;
using RescueRS.Presenter.Controllers.App.V1.Enums;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Domain.Application.Services.Interfaces;
using ResgateRS.Infrastructure.Repositories;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;
using ResgateRS.Tools;

namespace ResgateRS.Domain.Application.Services;

public class LoginService(UserRepository userRepository) : BaseService<UserRepository>(userRepository), IService
{
    public async Task<ActionResult<LoginResponseDTO>> handle(LoginRequestDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Celphone))
            return new BadRequestObjectResult(new ResponseDTO
            {
                DebugMessage = "Telefone é necessário",
                Message = "An error occurred, try again!"
            });

        UserEntity? user = await _mainRepository.GetUser(dto);

        if (user == null)
        {
            user = new UserEntity
            {
                Celphone = dto.Celphone,
                Rescuer = dto.Rescuer,
            };

            user = await _mainRepository.InsertOrUpdate(user);
        }

        Dictionary<string, object> claims = new()
        {
            { LoginClaimsEnum.UserId, user.UserId },
            { LoginClaimsEnum.Celphone, user.Celphone },
            { LoginClaimsEnum.Rescuer, user.Rescuer }
        };

        string token = JwtManager.GenerateToken(claims);

        return new OkObjectResult(new LoginResponseDTO
        {
            Token = token,
            Rescuer = user.Rescuer
        });
    }
}