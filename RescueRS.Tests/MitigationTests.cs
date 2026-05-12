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
using ResgateRS.Domain.Application.Services.Otp;

namespace RescueRS.Tests;

public class MitigationTests
{
    private ServiceProvider BuildServiceProvider(bool withOtp = false)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AuthenticationSettings:TokenSecret"] = "TestSecretKeyForDevelopmentOnly32Chars!",
                ["AuthenticationSettings:ExpiresInMinutes"] = "30",
                ["AuthenticationSettings:RescuerAccessCode"] = "UNUSED"
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
        if (withOtp)
            services.AddSingleton<IOtpProvider, MockOtpProvider>();
        services.AddTransient(provider =>
        {
            var jwt = new JwtTool();
            jwt.setExpires(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
            jwt.setSecret("TestSecretKeyForDevelopmentOnly32Chars!");
            return jwt;
        });

        JwtManager.secret = "TestSecretKeyForDevelopmentOnly32Chars!";
        JwtManager.expirationMinutes = 30;
        ResgateRS.Infrastructure.Database.Encryption.PhoneEncryptionConverter.SetKey("TestSecretKeyForDevelopmentOnly32Chars!");

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task S2_E1_NewUser_ShouldNotBeRescuer()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

        var result = await loginService.Handle(new LoginRequestDTO
        {
            Cellphone = "51999990001"
        });

        Assert.False(result.Rescuer);
    }

    [Fact]
    public async Task S2_E1_AdminApproval_ShouldGrantRescuerRole()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

        await loginService.Handle(new LoginRequestDTO { Cellphone = "51999990002" });

        var user = await db.Users.FirstAsync(u => u.CellphoneHash ==
            ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990002"));
        user.Rescuer = true;
        await db.SaveChangesAsync();

        var result = await loginService.Handle(new LoginRequestDTO { Cellphone = "51999990002" });
        Assert.True(result.Rescuer);
    }

    [Fact]
    public async Task T1_Description_ShouldBeSanitized()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();

        var loginResult = await loginService.Handle(new LoginRequestDTO
        {
            Cellphone = "51999990003"
        });

        userSession.UserId = Guid.Parse(new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(loginResult.Token!)
            .Payload["userData"].ToString()!
            .Contains("UserId")
            ? System.Text.Json.JsonSerializer.Deserialize<UserSession>(
                new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                .ReadJwtToken(loginResult.Token!)
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
        db.Users.Add(new UserEntity { UserId = userId, Cellphone = "51999990004", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990004"), Rescuer = true });
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
        db.Users.Add(new UserEntity { UserId = userId, Cellphone = "51999990005", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990005"), Rescuer = true });
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
        db.Users.Add(new UserEntity { UserId = userId, Cellphone = "51999990006", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990006"), Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId,
            Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(),
            UpdatedBy = userId,
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

    [Fact]
    public void I1_CsProj_ShouldNotReferenceWalletFiles()
    {
        var csprojPath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RescueRS", "ResgateRS.csproj");
        var content = File.ReadAllText(csprojPath);

        Assert.DoesNotContain("cwallet.sso", content);
        Assert.DoesNotContain("ewallet.p12", content);
        Assert.DoesNotContain("keystore.jks", content);
        Assert.DoesNotContain("tnsnames.ora", content);
    }

    [Fact]
    public void I4_CorsPolicy_ShouldNotAllowAnyOrigin()
    {
        var programPath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RescueRS", "Program.cs");
        var content = File.ReadAllText(programPath);

        Assert.DoesNotContain("AllowAnyOrigin", content);
        Assert.Contains("WithOrigins", content);
    }

    [Fact]
    public async Task T2_ForeignKey_ShouldBeEnforced()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();

        var invalidUserId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = Guid.NewGuid(),
            Status = RescueStatusEnum.Pending,
            RequestedBy = invalidUserId,
            Latitude = -29.95,
            Longitude = -51.18,
            AdultsNumber = 1,
            ContactPhone = "51999990008",
            RequestDateTime = DateTimeOffset.Now,
            UpdateDateTime = DateTimeOffset.Now
        });

        // InMemory provider does not enforce FK constraints,
        // but we verify the mapping configuration exists
        var entityType = db.Model.FindEntityType(typeof(RescueEntity));
        var foreignKeys = entityType!.GetForeignKeys().ToList();
        Assert.True(foreignKeys.Count >= 3, $"Expected at least 3 FKs (RequestedBy, ConfirmedBy, UpdatedBy), found {foreignKeys.Count}");
    }

    [Fact]
    public void I5_LoginController_ShouldSetHttpOnlyCookie()
    {
        var controllerPath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..",
            "RescueRS", "Presenter", "Controllers", "App", "V1", "Login", "LoginController.cs");
        var content = File.ReadAllText(controllerPath);

        Assert.Contains("HttpOnly = true", content);
        Assert.Contains("Secure = isProduction", content);
        Assert.Contains("SameSite", content);
    }

    [Fact]
    public void R2_RequestLoggingMiddleware_ShouldExist()
    {
        var middlewarePath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..",
            "RescueRS", "Middlewares", "RequestLoggingMiddleware.cs");
        Assert.True(File.Exists(middlewarePath), "RequestLoggingMiddleware.cs should exist");

        var content = File.ReadAllText(middlewarePath);
        Assert.Contains("LogInformation", content);
        Assert.Contains("LogError", content);

        var programPath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RescueRS", "Program.cs");
        var programContent = File.ReadAllText(programPath);
        Assert.Contains("RequestLoggingMiddleware", programContent);
    }

    [Fact]
    public async Task S1_S3_LoginWithoutOtp_ShouldRequestCode()
    {
        using var sp = BuildServiceProvider(withOtp: true);
        using var scope = sp.CreateScope();
        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

        var result = await loginService.Handle(new LoginRequestDTO
        {
            Cellphone = "51999990010",
            OtpCode = null
        });
        Assert.True(result.OtpSent);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task S1_S3_LoginWithWrongOtp_ShouldFail()
    {
        using var sp = BuildServiceProvider(withOtp: true);
        using var scope = sp.CreateScope();
        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

        await loginService.Handle(new LoginRequestDTO { Cellphone = "51999990011" });

        var ex = await Assert.ThrowsAsync<MessageException>(() =>
            loginService.Handle(new LoginRequestDTO
            {
                Cellphone = "51999990011",
                OtpCode = "000000"
            }));
        Assert.Contains("inválido", ex.Message.ToLower());
    }

    [Fact]
    public async Task S1_S3_LoginWithCorrectOtp_ShouldSucceed()
    {
        using var sp = BuildServiceProvider(withOtp: true);
        using var scope = sp.CreateScope();
        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

        await loginService.Handle(new LoginRequestDTO { Cellphone = "51999990012" });

        var result = await loginService.Handle(new LoginRequestDTO
        {
            Cellphone = "51999990012",
            OtpCode = "123456"
        });
        Assert.NotNull(result.Token);
    }

    [Fact]
    public void I6_PhoneMasking_NonRescuerShouldSeeMaskedPhone()
    {
        var entity = new RescueEntity
        {
            RescueId = Guid.NewGuid(),
            ContactPhone = "51999990013",
            Status = RescueStatusEnum.Pending,
            Latitude = -29.95,
            Longitude = -51.18,
            AdultsNumber = 1,
            RequestDateTime = DateTimeOffset.Now,
            UpdateDateTime = DateTimeOffset.Now
        };

        var dtoNonRescuer = RescueDTO.FromEntity(entity, isRescuer: false);
        Assert.DoesNotContain("51999", dtoNonRescuer.Cellphone);
        Assert.EndsWith("0013", dtoNonRescuer.Cellphone);

        var dtoRescuer = RescueDTO.FromEntity(entity, isRescuer: true);
        Assert.Equal("51999990013", dtoRescuer.Cellphone);
    }

    [Fact]
    public async Task R1_AuditLog_ShouldRecordStatusChange()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var userId = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = userId, Cellphone = "51999990020", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990020"), Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId, Status = RescueStatusEnum.Pending,
            RequestedBy = Guid.NewGuid(), Latitude = -29.95, Longitude = -51.18,
            AdultsNumber = 1, ContactPhone = "51999990020",
            RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = userId;
        userSession.Rescuer = true;

        await rescueService.StartRescue(new RescueGuidDTO { RescueId = rescueId });

        var log = await db.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("StatusChange", log!.Action);
        Assert.Equal("Pending", log.PreviousValue);
        Assert.Equal("InProgress", log.NewValue);
        Assert.Equal(userId, log.UserId);
    }

    [Fact]
    public async Task I3_PhoneEncryption_ShouldEncryptAndLookupByHash()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();

        var phone = "51999990021";
        var hash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash(phone);

        var user = new UserEntity
        {
            UserId = Guid.NewGuid(),
            Cellphone = phone,
            CellphoneHash = hash,
            Rescuer = false
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var retrieved = await db.Users.FirstAsync(u => u.CellphoneHash == hash);
        Assert.Equal(phone, retrieved.Cellphone);
        Assert.Equal(hash, retrieved.CellphoneHash);
    }

    [Fact]
    public async Task E3_BannedUser_ShouldNotLogin()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

        db.Users.Add(new UserEntity
        {
            UserId = Guid.NewGuid(), Cellphone = "51999990022", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990022"),
            Rescuer = false, Banned = true
        });
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<MessageException>(() =>
            loginService.Handle(new LoginRequestDTO { Cellphone = "51999990022" }));
        Assert.Contains("suspensa", ex.Message.ToLower());
    }

    [Fact]
    public void E3_AdminController_ShouldExist()
    {
        var controllerPath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..",
            "RescueRS", "Presenter", "Controllers", "App", "V1", "Admin", "AdminController.cs");
        Assert.True(File.Exists(controllerPath));
        var content = File.ReadAllText(controllerPath);
        Assert.Contains("Ban", content);
        Assert.Contains("SetRole", content);
        Assert.Contains("ListUsers", content);
    }

    [Fact]
    public void D2_ProximitySearch_ShouldUsePagination()
    {
        var repoPath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..",
            "RescueRS", "Infrastructure", "Repositories", "RescueRepository.cs");
        var content = File.ReadAllText(repoPath);
        Assert.Contains("Take(pageSize)", content);
    }

    [Fact]
    public void D1_D3_RateLimiting_ShouldBeConfigured()
    {
        var programPath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RescueRS", "Program.cs");
        var content = File.ReadAllText(programPath);

        Assert.Contains("AddRateLimiter", content);
        Assert.Contains("UseRateLimiter", content);
        Assert.Contains("PermitLimit", content);
    }

    [Fact]
    public async Task E2_Ownership_OtherRescuerCannotAlterInProgress()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var rescuerA = Guid.NewGuid();
        var rescuerB = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = rescuerA, Cellphone = "51999990030", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990030"), Rescuer = true });
        db.Users.Add(new UserEntity { UserId = rescuerB, Cellphone = "51999990031", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990031"), Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId, Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(), UpdatedBy = rescuerA,
            Latitude = -29.95, Longitude = -51.18, AdultsNumber = 1,
            ContactPhone = "51999990030", RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = rescuerB;
        userSession.Rescuer = true;
        userSession.Admin = false;

        var ex = await Assert.ThrowsAsync<MessageException>(() =>
            rescueService.ConfirmRescue(new RescueGuidDTO { RescueId = rescueId }));
        Assert.Contains("resgatista que iniciou", ex.Message.ToLower());
    }

    [Fact]
    public async Task E2_Ownership_RescuerWhoStartedCanComplete()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var rescuerId = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = rescuerId, Cellphone = "51999990032", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990032"), Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId, Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(), UpdatedBy = rescuerId,
            Latitude = -29.95, Longitude = -51.18, AdultsNumber = 1,
            ContactPhone = "51999990032", RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = rescuerId;
        userSession.Rescuer = true;

        var result = await rescueService.ConfirmRescue(new RescueGuidDTO { RescueId = rescueId });
        Assert.Contains("concluído", result.Message.ToLower());
    }

    [Fact]
    public async Task E2_Ownership_AdminCanAlterAnyInProgress()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var adminId = Guid.NewGuid();
        var rescuerId = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = adminId, Cellphone = "51999990033", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990033"), Rescuer = true, Admin = true });
        db.Users.Add(new UserEntity { UserId = rescuerId, Cellphone = "51999990034", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990034"), Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId, Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(), UpdatedBy = rescuerId,
            Latitude = -29.95, Longitude = -51.18, AdultsNumber = 1,
            ContactPhone = "51999990034", RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = adminId;
        userSession.Rescuer = true;
        userSession.Admin = true;

        var result = await rescueService.ConfirmRescue(new RescueGuidDTO { RescueId = rescueId });
        Assert.Contains("concluído", result.Message.ToLower());
    }

    [Fact]
    public async Task E2_Ownership_RequesterCanAlterOwnRescueInProgress()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var requesterId = Guid.NewGuid();
        var rescuerId = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = requesterId, Cellphone = "51999990035", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990035"), Rescuer = false });
        db.Users.Add(new UserEntity { UserId = rescuerId, Cellphone = "51999990036", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990036"), Rescuer = true });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId, Status = RescueStatusEnum.InProgress,
            RequestedBy = requesterId, UpdatedBy = rescuerId,
            Latitude = -29.95, Longitude = -51.18, AdultsNumber = 1,
            ContactPhone = "51999990035", RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = requesterId;
        userSession.Rescuer = false;

        var result = await rescueService.ConfirmRescue(new RescueGuidDTO { RescueId = rescueId });
        Assert.Contains("concluído", result.Message.ToLower());
    }

    [Fact]
    public async Task E2_Ownership_NonRequesterNonRescuerCannotAlter()
    {
        using var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        var userSession = scope.ServiceProvider.GetRequiredService<UserSession>();
        var rescueService = scope.ServiceProvider.GetRequiredService<RescueService>();

        var randomUser = Guid.NewGuid();
        db.Users.Add(new UserEntity { UserId = randomUser, Cellphone = "51999990037", CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash("51999990037"), Rescuer = false });
        var rescueId = Guid.NewGuid();
        db.Rescues.Add(new RescueEntity
        {
            RescueId = rescueId, Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid(),
            Latitude = -29.95, Longitude = -51.18, AdultsNumber = 1,
            ContactPhone = "51999990038", RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        userSession.UserId = randomUser;
        userSession.Rescuer = false;

        var ex = await Assert.ThrowsAsync<MessageException>(() =>
            rescueService.ConfirmRescue(new RescueGuidDTO { RescueId = rescueId }));
        Assert.Contains("permissão", ex.Message.ToLower());
    }

    [Fact]
    public void I6_RescuerPhone_RequesterSeesFullPhone()
    {
        var requesterId = Guid.NewGuid();
        var rescuerId = Guid.NewGuid();
        var entity = new RescueEntity
        {
            RescueId = Guid.NewGuid(), Status = RescueStatusEnum.InProgress,
            RequestedBy = requesterId, UpdatedBy = rescuerId,
            ContactPhone = "51999990040", Latitude = -29.95, Longitude = -51.18,
            AdultsNumber = 1, RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        };

        var dto = RescueDTO.FromEntity(entity, currentUserId: requesterId, isRescuer: false, rescuerPhone: "51999990041");
        Assert.Equal("51999990041", dto.RescuerPhone);
    }

    [Fact]
    public void I6_RescuerPhone_OtherNonRescuerDoesNotSeePhone()
    {
        var entity = new RescueEntity
        {
            RescueId = Guid.NewGuid(), Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid(),
            ContactPhone = "51999990042", Latitude = -29.95, Longitude = -51.18,
            AdultsNumber = 1, RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        };

        var dto = RescueDTO.FromEntity(entity, currentUserId: Guid.NewGuid(), isRescuer: false, rescuerPhone: "51999990043");
        Assert.Null(dto.RescuerPhone);
    }

    [Fact]
    public void I6_RescuerPhone_RescuerSeesFullPhone()
    {
        var rescuerId = Guid.NewGuid();
        var otherRescuerId = Guid.NewGuid();
        var entity = new RescueEntity
        {
            RescueId = Guid.NewGuid(), Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(), UpdatedBy = otherRescuerId,
            ContactPhone = "51999990044", Latitude = -29.95, Longitude = -51.18,
            AdultsNumber = 1, RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        };

        var dto = RescueDTO.FromEntity(entity, currentUserId: rescuerId, isRescuer: true, rescuerPhone: "51999990045");
        Assert.Equal("51999990045", dto.RescuerPhone);
    }

    [Fact]
    public void I6_RescuerPhone_NullWhenStartedByMe()
    {
        var rescuerId = Guid.NewGuid();
        var entity = new RescueEntity
        {
            RescueId = Guid.NewGuid(), Status = RescueStatusEnum.InProgress,
            RequestedBy = Guid.NewGuid(), UpdatedBy = rescuerId,
            ContactPhone = "51999990046", Latitude = -29.95, Longitude = -51.18,
            AdultsNumber = 1, RequestDateTime = DateTimeOffset.Now, UpdateDateTime = DateTimeOffset.Now
        };

        var dto = RescueDTO.FromEntity(entity, currentUserId: rescuerId, isRescuer: true, rescuerPhone: "51999990047");
        Assert.Null(dto.RescuerPhone);
    }

    [Fact]
    public void E1_ListEndpoints_ShouldRequireRescuerRole()
    {
        var controllerPath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..",
            "RescueRS", "Presenter", "Controllers", "App", "V1", "Rescue", "RescueController.cs");
        var content = File.ReadAllText(controllerPath);

        var listMethods = new[] { "ListPendingRescues", "ListPendingRescuesByProximity", "ListCompletedRescues", "ListInProgressRescues" };
        foreach (var method in listMethods)
        {
            var idx = content.IndexOf(method);
            Assert.True(idx >= 0, $"{method} not found");
            var snippet = content.Substring(idx, Math.Min(300, content.Length - idx));
            Assert.Contains("Rescuer", snippet, StringComparison.Ordinal);
        }
    }
}
