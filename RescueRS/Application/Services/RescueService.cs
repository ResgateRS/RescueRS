﻿using Microsoft.AspNetCore.Mvc;
using ResgateRS.Presenter.Controllers.App.V1.Enums;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Domain.Application.Services.Interfaces;
using ResgateRS.Infrastructure.Repositories;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;
using ResgateRS.Tools;
using ResgateRS.Auth;
using ResgateRS.DTOs;

namespace ResgateRS.Domain.Application.Services;

public class RescueService(IServiceProvider serviceProvider, UserSession userSession) : BaseService(serviceProvider, userSession), IService
{
    public async Task<IResponse<object>> ConfirmRescue(RescueConfirmDTO dto)
    {
        if (dto.RescueId == Guid.Empty)
            throw new Exception("RescueId is required");

        RescueEntity? rescue = await _serviceProvider.GetRequiredService<RescueRepository>().GetRescueById(dto.RescueId);

        if (rescue == null)
            throw new Exception("Rescue not found");

        rescue.RescueDateTime = DateTimeOffset.Now;
        rescue.Rescued = true;
        rescue.ConfirmedBy = _userSession.UserId;

        await _serviceProvider.GetRequiredService<RescueRepository>().InsertOrUpdate(rescue);

        return Response.Success(default(Object), "Resgate concluído!");
    }

    public async Task<IResponse<RescueDTO>> DetailRescue(Guid rescueId)
    {
        if (rescueId == Guid.Empty)
            throw new Exception("RescueId is required");

        RescueEntity? rescue = await _serviceProvider.GetRequiredService<RescueRepository>().GetRescueById(rescueId);

        if (rescue == null)
            throw new Exception("Rescue not found");

        return Response.Success(RescueDTO.FromEntity(rescue));
    }

    public async Task<IResponse<IEnumerable<RescueCardDTO>>> ListPendingRescues()
    {
        IEnumerable<RescueEntity> rescues = await _serviceProvider.GetRequiredService<RescueRepository>().GetPendingRescues();

        return Response.Success(rescues.Select(RescueCardDTO.FromEntity));
    }

    public async Task<IResponse<IEnumerable<RescueCardDTO>>> ListPendingRescuesByProximity(double latitude, double longitude)
    {
        IEnumerable<RescueEntity> rescues = await _serviceProvider.GetRequiredService<RescueRepository>().GetPendingRescuesByProximity(latitude, longitude);

        return Response.Success(rescues.Select(RescueCardDTO.FromEntity));
    }

    public async Task<IResponse<IEnumerable<RescueCardDTO>>> ListMyRescues()
    {
        IEnumerable<RescueEntity> rescues = await _serviceProvider.GetRequiredService<RescueRepository>().GetMyRescues(_userSession.UserId, _userSession.Rescuer);

        return Response.Success(rescues.Select(RescueCardDTO.FromEntity));
    }

    public async Task<IResponse<ResponseDTO>> RequestRescue(RescueRequestDTO dto)
    {
        if (dto.TotalPeopleNumber <= 0)
            throw new Exception("TotalPeopleNumber is required");

        if (dto.AnimalsNumber < 0 || dto.ChildrenNumber < 0 || dto.DisabledNumber < 0 || dto.ElderlyNumber < 0)
            throw new Exception("Negative number of people");

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
            RequestedBy = _userSession.UserId,
            ContactPhone = _userSession.Cellphone
        };

        await _serviceProvider.GetRequiredService<RescueRepository>().InsertOrUpdate(entity);

        return Response.Success(new ResponseDTO { Message = "Solicitação de Resgate Registrada!" });
    }
}