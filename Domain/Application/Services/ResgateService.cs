using Microsoft.AspNetCore.Mvc;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Domain.Application.Services.Interfaces;
using ResgateRS.Infrastructure.Repositories;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;
using ResgateRS.Tools;

namespace ResgateRS.Domain.Application.Services;

public class ResgateService(RescueRepository resgateRepository) : BaseService<RescueRepository>(resgateRepository), IService
{
    public async Task<ActionResult<ResponseDTO>> ConfirmRescue(RescueConfirmDTO dto, string authToken)
    {
        if (dto.RescueId == Guid.Empty)
            return new BadRequestObjectResult(new ResponseDTO
            {
                DebugMessage = "RescueId is required",
                Message = "Um erro aconteceu, tente novamente!"
            });

        Guid userId = JwtManager.ExtractPayload<Guid>(authToken.Split(' ')[1], "UserId");
        if (userId == Guid.Empty)
            return new UnauthorizedObjectResult(new ResponseDTO
            {
                DebugMessage = "Invalid token",
                Message = "Ocorreu um erro, tente novamente!"
            });

        RescueEntity? rescue = await _mainRepository.GetRescueById(dto.RescueId);

        if (rescue == null)
            return new NotFoundObjectResult(new ResponseDTO { Message = "Rescue not found" });

        rescue.RescueDateTime = dto.ConfirmationDateTime ?? DateTimeOffset.Now;
        rescue.Rescued = true;
        rescue.ConfirmedBy = userId;

        await _mainRepository.InsertOrUpdate(rescue);

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

        RescueEntity? rescue = await _mainRepository.GetRescueById(rescueId);

        if (rescue == null)
            return new NotFoundObjectResult(new ResponseDTO
            {
                Message = "Um erro aconteceu, tente novamente!",
                DebugMessage = "Rescue not found"
            });

        return new OkObjectResult(RescueDTO.FromEntity(rescue));
    }

    public async Task<ActionResult<IEnumerable<RescueCardDTO>>> ListPendingRescues(int page, int size)
    {
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

        IEnumerable<RescueEntity> rescues = await _mainRepository.GetPendingRescues(page, size);

        return new OkObjectResult(rescues.Select(RescueCardDTO.FromEntity));
    }

    public async Task<ActionResult<IEnumerable<RescueCardDTO>>> ListMyRescues(int page, int size, string authToken)
    {
        Guid userId = JwtManager.ExtractPayload<Guid>(authToken.Split(' ')[1], "UserId");
        bool rescuer = JwtManager.ExtractPayload<bool>(authToken.Split(' ')[1], "Rescuer");

        if (userId == Guid.Empty)
            return new UnauthorizedObjectResult(new ResponseDTO
            {
                DebugMessage = "Invalid token",
                Message = "Ocorreu um erro, tente novamente!"
            });

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

        IEnumerable<RescueEntity> rescues = await _mainRepository.GetMyRescues(page, size, userId, rescuer);

        return new OkObjectResult(rescues.Select(RescueCardDTO.FromEntity));
    }

    public async Task<ActionResult<ResponseDTO>> RequestRescue(RescueRequestDTO dto, string authToken)
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

        Guid? userId = JwtManager.ExtractPayload<Guid>(authToken.Split(' ')[1], "UserId");
        string? phone = JwtManager.ExtractPayload<string>(authToken.Split(' ')[1], "Celphone");

        if (userId == Guid.Empty || string.IsNullOrEmpty(phone))
            return new UnauthorizedObjectResult(new ResponseDTO
            {
                DebugMessage = "Invalid token",
                Message = "Ocorreu um erro, tente novamente!"
            });

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
            RequestedBy = userId!.Value,
            ContactPhone = phone!
        };

        await _mainRepository.InsertOrUpdate(entity);

        return new OkObjectResult(new ResponseDTO { Message = "Solicitação de Resgate Registrada!" });
    }
}