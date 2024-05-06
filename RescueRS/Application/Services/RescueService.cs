using Microsoft.AspNetCore.Mvc;
using ResgateRS.Presenter.Controllers.App.V1.Enums;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Domain.Application.Services.Interfaces;
using ResgateRS.Infrastructure.Repositories;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;
using ResgateRS.Tools;
using ResgateRS.Auth;

namespace ResgateRS.Domain.Application.Services;

public class RescueService(IServiceProvider serviceProvider, UserSession userSession) : BaseService(serviceProvider, userSession), IService
{
    public async Task<ActionResult<ResponseDTO>> ConfirmRescue(RescueConfirmDTO dto)
    {
        if (dto.RescueId == Guid.Empty)
            return new BadRequestObjectResult(new ResponseDTO
            {
                DebugMessage = "RescueId is required",
                Message = "Um erro aconteceu, tente novamente!"
            });

        // Guid userId = JwtManager.ExtractPayload<Guid>(authToken.Split(' ')[1], LoginClaimsEnum.UserId);
        // if (userId == Guid.Empty)
        //     return new UnauthorizedObjectResult(new ResponseDTO
        //     {
        //         DebugMessage = "Invalid token",
        //         Message = "Ocorreu um erro, tente novamente!"
        //     });

        RescueEntity? rescue = await _serviceProvider.GetRequiredService<RescueRepository>().GetRescueById(dto.RescueId);

        if (rescue == null)
            return new NotFoundObjectResult(new ResponseDTO { Message = "Rescue not found" });

        rescue.RescueDateTime = DateTimeOffset.Now;
        rescue.Rescued = true;
        rescue.ConfirmedBy = Guid.Empty;
        // rescue.ConfirmedBy = userId;

        await _serviceProvider.GetRequiredService<RescueRepository>().InsertOrUpdate(rescue);

        return new OkObjectResult(new ResponseDTO { Message = "Resgate Concluido!" });
    }

    public async Task<ActionResult<RescueDTO>> DetailRescue(Guid rescueId)
    {
        if (rescueId == Guid.Empty)
            return new BadRequestObjectResult(new ResponseDTO
            {
                DebugMessage = "RescueId is required",
                Message = "Um erro aconteceu, tente novamente!"
            });

        RescueEntity? rescue = await _serviceProvider.GetRequiredService<RescueRepository>().GetRescueById(rescueId);

        if (rescue == null)
            return new NotFoundObjectResult(new ResponseDTO
            {
                Message = "Um erro aconteceu, tente novamente!",
                DebugMessage = "Rescue not found"
            });

        return new OkObjectResult(RescueDTO.FromEntity(rescue));
    }

    public async Task<ActionResult<IEnumerable<RescueCardDTO>>> ListPendingRescues()
    {
        // if (page < 1)
        //     return new BadRequestObjectResult(new ResponseDTO
        //     {
        //         DebugMessage = "Page must be greater than 0",
        //         Message = "Um erro aconteceu, tente novamente!"
        //     });

        // if (size < 1)
        //     return new BadRequestObjectResult(new ResponseDTO
        //     {
        //         Message = "Um erro aconteceu, tente novamente!",
        //         DebugMessage = "Size must be greater than 0"
        //     });

        IEnumerable<RescueEntity> rescues = await _serviceProvider.GetRequiredService<RescueRepository>().GetPendingRescues();

        return new OkObjectResult(rescues.Select(RescueCardDTO.FromEntity));
    }

    public async Task<ActionResult<IEnumerable<RescueCardDTO>>> ListPendingRescuesByProximity(double latitude, double longitude)
    {
        // if (page < 1)
        //     return new BadRequestObjectResult(new ResponseDTO
        //     {
        //         DebugMessage = "Page must be greater than 0",
        //         Message = "Um erro aconteceu, tente novamente!"
        //     });

        // if (size < 1)
        //     return new BadRequestObjectResult(new ResponseDTO
        //     {
        //         Message = "Um erro aconteceu, tente novamente!",
        //         DebugMessage = "Size must be greater than 0"
        //     });

        IEnumerable<RescueEntity> rescues = await _serviceProvider.GetRequiredService<RescueRepository>().GetPendingRescuesByProximity(latitude, longitude);

        // if (latitude != null && longitude != null)
        //     rescues = rescues.OrderBy(x => x.GetDistance(latitude.Value, longitude.Value));

        return new OkObjectResult(rescues.Select(RescueCardDTO.FromEntity));
    }

    public async Task<ActionResult<IEnumerable<RescueCardDTO>>> ListMyRescues(int page, int size)
    {
        // Guid userId = JwtManager.ExtractPayload<Guid>(authToken.Split(' ')[1], LoginClaimsEnum.UserId);
        // bool rescuer = JwtManager.ExtractPayload<bool>(authToken.Split(' ')[1], LoginClaimsEnum.Rescuer);

        // if (userId == Guid.Empty)
        //     return new UnauthorizedObjectResult(new ResponseDTO
        //     {
        //         DebugMessage = "Invalid token",
        //         Message = "Ocorreu um erro, tente novamente!"
        //     });

        if (page < 1)
            return new BadRequestObjectResult(new ResponseDTO
            {
                DebugMessage = "Page must be greater than 0",
                Message = "Um erro aconteceu, tente novamente!"
            });

        if (size < 1)
            return new BadRequestObjectResult(new ResponseDTO
            {
                Message = "Um erro aconteceu, tente novamente!",
                DebugMessage = "Size must be greater than 0"
            });

        IEnumerable<RescueEntity> rescues = await _serviceProvider.GetRequiredService<RescueRepository>().GetMyRescues(page, size, /*_userSession.UserIdGuid*/Guid.Empty, _userSession.Rescuer);

        return new OkObjectResult(rescues.Select(RescueCardDTO.FromEntity));
    }

    public async Task<ActionResult<ResponseDTO>> RequestRescue(RescueRequestDTO dto)
    {
        if (dto.TotalPeopleNumber <= 0)
            return new BadRequestObjectResult(new ResponseDTO
            {
                DebugMessage = "TotalPeopleNumber is required",
                Message = "O número de pessoas é obrigatório"
            });

        if (dto.AnimalsNumber < 0 || dto.ChildrenNumber < 0 || dto.DisabledNumber < 0 || dto.ElderlyNumber < 0)
            return new BadRequestObjectResult(new ResponseDTO
            {
                DebugMessage = "Negative number of people",
                Message = "O número de pessoas não pode ser negativo"
            });

        // Guid? userId = JwtManager.ExtractPayload<Guid>(authToken.Split(' ')[1], LoginClaimsEnum.UserId);
        // string? phone = JwtManager.ExtractPayload<string>(authToken.Split(' ')[1], LoginClaimsEnum.Cellphone);

        // if (userId == Guid.Empty || string.IsNullOrEmpty(phone))
        //     return new UnauthorizedObjectResult(new ResponseDTO
        //     {
        //         DebugMessage = "Invalid token",
        //         Message = "Ocorreu um erro, tente novamente!"
        //     });

        RescueEntity entity = new()
        {
            RequestDateTime = DateTimeOffset.Now,
            TotalPeopleNumber = dto.TotalPeopleNumber,
            ChildrenNumber = dto.ChildrenNumber,
            ElderlyNumber = dto.ElderlyNumber,
            DisabledNumber = dto.DisabledNumber,
            AnimalsNumber = dto.AnimalsNumber,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Rescued = false,
            RequestedBy = Guid.Empty,//userId!.Value,
            ContactPhone = "1231231"
        };

        await _serviceProvider.GetRequiredService<RescueRepository>().InsertOrUpdate(entity);

        return new OkObjectResult(new ResponseDTO { Message = "Solicitação de Resgate Registrada!" });
    }
}