using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RescueRS.Application.Enums;
using ResgateRS.Auth;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Domain.Application.Services;
using ResgateRS.DTOs;
using ResgateRS.Infrastructure.Database;
using ResgateRS.Infrastructure.Repositories;
using ResgateRS.Middleware;
using ResgateRS.Pagination;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;
using ResgateRS.Tools;

namespace RescueRS.Tests;

public class MitigationTests
{
    private ServiceProvider BuildServiceProvider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AuthenticationSettings:TokenSecret"] = "TestSecretKeyForDevelopmentOnly32Chars!",
                ["AuthenticationSettings:ExpiresInMinutes"] = "30",
                ["AuthenticationSettings:RescuerAccessCode"] = "RESGATE2024"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddDbContext<ResgateRSDbContext>(o => o.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        services.AddSingleton<IConfiguration>(config);
        services.AddScoped<UserSession>();
        services.AddScoped<PaginationDTO>();
        services.AddScoped<UserRepository>();
        services.AddScoped<RescueRepository>();
        services.AddScoped<LoginService>();
        services.AddScoped<RescueService>();
        services.AddTransient(provider =>
        {
            var jwt = new JwtTool();
            jwt.setExpires(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
            jwt.setSecret("TestSecretKeyForDevelopmentOnly32Chars!");
            return jwt;
        });

        JwtManager.secret = "TestSecretKeyForDevelopmentOnly32Chars!";
        JwtManager.expirationMinutes = 30;

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task S2_E1_LoginWithoutCode_ShouldNotGrantRescuerRole()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

        var result = await loginService.handle(new LoginRequestDTO
        {
            Cellphone = "51999990001",
            Rescuer = true,
            RescuerAccessCode = null
        });

        Assert.False(result.Data!.Rescuer);
    }

    [Fact]
    public async Task S2_E1_LoginWithCode_ShouldGrantRescuerRole()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

        var result = await loginService.handle(new LoginRequestDTO
        {
            Cellphone = "51999990002",
            Rescuer = true,
            RescuerAccessCode = "RESGATE2024"
        });

        Assert.True(result.Data!.Rescuer);
    }

    [Fact]
    public async Task T1_Description_ShouldBeSanitized()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();

        var loginResult = await loginService.handle(new LoginRequestDTO
        {
            Cellphone = "51999990003",
            Rescuer = false
        });

        userSession.UserId = Guid.Parse(new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(loginResult.Data!.Token)
            .Payload["userData"].ToString()!
            .Contains("UserId")
            ? System.Text.Json.JsonSerializer.Deserialize<UserSession>(
                new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                .ReadJwtToken(loginResult.Data.Token)
                .Payload["userData"].ToString()!)!.UserId.ToString()
            : Guid.Empty.ToString());
        userSession.Rescuer = false;
        userSession.Cellphone = "51999990003";

        await rescueService.RequestRescue(new RescueRequestDTO
        {
            ContactPhone = "51999990003",
            Description = "<script>alert(1)</script>Preciso de ajuda",
            AdultsNumber = 1,
            ChildrenNumber = 0,
            ElderlyNumber = 0,
            DisabledNumber = 0,
            AnimalsNumber = 0,
            Latitude = -29.95,
            Longitude = -51.18
        });

        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var rescue = await db.Rescues.FirstAsync();
        Assert.DoesNotContain("<script>", rescue.Description);
        Assert.Contains("Preciso de ajuda", rescue.Description);
    }

    [Fact]
    public async Task T3_E2_InvalidTransition_PendingToCompleted_ShouldThrow()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var userId = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = userId, Cellphone = "51999990004", Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId,
            Status = RescueStatusEnum.Pending,
            RequestedBy = Guid.NewGuid(),
            Latitude = -29.95,
            Longitude = -51.18,
            AdultsNumber = 1,
            ContactPhone = "51999990004",
            RequestDateTime = DateTimeOffset.Now,
            UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = userId;
        userSession.Rescuer = true;

        var ex = await Assert.ThrowsAsync<MessageException>(() =>
            rescueService.ConfirmRescue(new RescueGuidDTO { RescueId = rescueId }));
        Assert.Contains("inválida", ex.Message.ToLower());
    }

    [Fact]
    public async Task T3_E2_ValidTransition_PendingToInProgress_ShouldSucceed()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var userId = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = userId, Cellphone = "51999990005", Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId,
            Status = RescueStatusEnum.Pending,
            RequestedBy = Guid.NewGuid(),
            Latitude = -29.95,
            Longitude = -51.18,
            AdultsNumber = 1,
            ContactPhone = "51999990005",
            RequestDateTime = DateTimeOffset.Now,
            UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = userId;
        userSession.Rescuer = true;

        var result = await rescueService.StartRescue(new RescueGuidDTO { RescueId = rescueId });
        Assert.Contains("iniciado", result.Message.ToLower());
    }

    [Fact]
    public async Task T3_E2_ValidTransition_InProgressToCompleted_ShouldSucceed()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var userId = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = userId, Cellphone = "51999990006", Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId,
            Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(),
            Latitude = -29.95,
            Longitude = -51.18,
            AdultsNumber = 1,
            ContactPhone = "51999990006",
            RequestDateTime = DateTimeOffset.Now,
            UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = userId;
        userSession.Rescuer = true;

        var result = await rescueService.ConfirmRescue(new RescueGuidDTO { RescueId = rescueId });
        Assert.Contains("concluído", result.Message.ToLower());
    }

    [Fact]
    public async Task I2_ErrorResponse_ShouldNotContainStackTrace()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        userSession.UserId = Guid.NewGuid();
        userSession.Rescuer = false;
        userSession.Cellphone = "51999990007";

        var ex = await Assert.ThrowsAsync<MessageException>(() =>
            rescueService.RequestRescue(new RescueRequestDTO
            {
                ContactPhone = "invalid",
                AdultsNumber = 0,
                ChildrenNumber = 0,
                ElderlyNumber = 0,
                DisabledNumber = 0,
                AnimalsNumber = 0,
                Latitude = 0,
                Longitude = 0
            }));

        Assert.DoesNotContain("at ResgateRS", ex.Message);
        Assert.DoesNotContain("StackTrace", ex.Message);
    }
}
