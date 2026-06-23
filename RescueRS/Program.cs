using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.OpenApi.Models;
using ResgateRS.Extensions;
using ResgateRS.Infrastructure.Database;
using ResgateRS.Tools;
using ResgateRS.Pagination;
using ResgateRS.Middleware;
using ResgateRS.SwaggerCustomizations;
using ResgateRS.Auth;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

JwtManager.secret = config["AuthenticationSettings:TokenSecret"]!;
ResgateRS.Infrastructure.Database.Encryption.PhoneEncryptionConverter.SetKey(
    config["AuthenticationSettings:TokenSecret"]!);
JwtManager.expirationMinutes = Convert.ToInt64(config["AuthenticationSettings:ExpiresInMinutes"]);

try
{
    StartApplication();
}
catch (Exception ex)
{
    Console.WriteLine($"The application failed to start correctly: {ex.Message}");
}

void StartApplication()
{
    ConfigureBuilder(builder);

    ConfigureServices(builder.Services);

    Configure(builder.Build());
}

void ConfigureBuilder(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "allowedOrigins",
                          policy =>
                          {
                              policy.WithOrigins("https://resgaters.app.br", "https://www.resgaters.app.br", "http://localhost:3000", "http://192.168.15.15:3000")
                                    .AllowCredentials()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          });
    });
}

void ConfigureServices(IServiceCollection services)
{
    services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = 429;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = builder.Environment.EnvironmentName == "Test" ? 500 : 120,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });

    services.AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        o.ReportApiVersions = true;
    });
    services.AddVersionedApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });
    services.AddControllers().AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "ResgateRS", Version = "v1" });

        c.AddSecurityDefinition("Bearer", SwaggerSecurity.SecurityScheme);
        c.AddSecurityRequirement(SwaggerSecurity.SecurityRequirement);

        c.OperationFilter<AddPaginationHeaderParameter>();
    });

    var dbProvider = builder.Configuration["DatabaseProvider"] ?? "Oracle";
    if (dbProvider == "InMemory")
    {
        services.AddDbContext<ResgateRSDbContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase("ResgateRS"));
        services.AddSingleton<ResgateRS.Domain.Application.Services.Otp.IOtpProvider, ResgateRS.Domain.Application.Services.Otp.MockOtpProvider>();
    }
    else
    {
        services.AddDbContext<ResgateRSDbContext>(optionsBuilder => optionsBuilder.UseOracle(builder.Configuration.GetConnectionString("ResgateRSDbContext")));
    }

    services.RegisterRepositoriesAndServices();

    services.AddScoped<UserSession>();

    services.AddScoped<PaginationDTO>();

    services.AddTransient(provider =>
        {
            int expiresInMinutes = int.Parse(builder.Configuration["AuthenticationSettings:ExpiresInMinutes"]!);
            long expiresTimestamp = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes).ToUnixTimeSeconds();
            JwtTool jwt = new JwtTool();
            jwt.setExpires(expiresTimestamp);
            jwt.setSecret(builder.Configuration["AuthenticationSettings:TokenSecret"]!);
            return jwt;
        });
}

void Configure(WebApplication app)
{
    if (app.Environment.EnvironmentName == "Test")
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResgateRSDbContext>();
        db.Database.EnsureCreated();
        if (!db.Users.Any(u => u.Admin))
        {
            var adminPhone = "51999000000";
            var adminId = Guid.NewGuid();
            db.Users.Add(new ResgateRS.Domain.Application.Entities.UserEntity
            {
                UserId = adminId,
                Cellphone = adminPhone,
                CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash(adminPhone),
                Rescuer = true,
                Admin = true,
            });

            var rescuerPhone = "51988880001";
            var rescuerId = Guid.NewGuid();
            db.Users.Add(new ResgateRS.Domain.Application.Entities.UserEntity
            {
                UserId = rescuerId,
                Cellphone = rescuerPhone,
                CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash(rescuerPhone),
                Rescuer = true,
                Admin = false,
            });

            var solicitanteIds = new Guid[5];
            var solicitantePhones = new[] { "51988001111", "51988002222", "51988003333", "51988004444", "51988005555" };
            for (int i = 0; i < 5; i++)
            {
                solicitanteIds[i] = Guid.NewGuid();
                db.Users.Add(new ResgateRS.Domain.Application.Entities.UserEntity
                {
                    UserId = solicitanteIds[i],
                    Cellphone = solicitantePhones[i],
                    CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash(solicitantePhones[i]),
                    Rescuer = false,
                    Admin = false,
                });
            }

            var pendingPhone = "51988009999";
            var pendingId = Guid.NewGuid();
            db.Users.Add(new ResgateRS.Domain.Application.Entities.UserEntity
            {
                UserId = pendingId,
                Cellphone = pendingPhone,
                CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash(pendingPhone),
                Rescuer = false,
                Admin = false,
                RescuerRequested = true,
                RescuerRequestDescription = "Sou voluntário da Defesa Civil e tenho barco próprio.",
            });

            db.Rescues.AddRange(
                new ResgateRS.Domain.Application.Entities.RescueEntity
                {
                    RescueId = Guid.NewGuid(),
                    RequestDateTime = DateTimeOffset.UtcNow.AddHours(-3),
                    AdultsNumber = 3, ChildrenNumber = 2, ElderlyNumber = 1, DisabledNumber = 0, AnimalsNumber = 1,
                    Latitude = -29.9545, Longitude = -51.1066,
                    Status = RescueRS.Application.Enums.RescueStatusEnum.Pending,
                    ContactPhone = "51988001111",
                    Description = "Família ilhada no segundo andar. Água subindo. 3 adultos, 2 crianças e 1 idoso acamado.",
                    RequestedBy = solicitanteIds[0], ConfirmedBy = Guid.Empty, UpdatedBy = solicitanteIds[0],
                },
                new ResgateRS.Domain.Application.Entities.RescueEntity
                {
                    RescueId = Guid.NewGuid(),
                    RequestDateTime = DateTimeOffset.UtcNow.AddHours(-2),
                    AdultsNumber = 2, ChildrenNumber = 0, ElderlyNumber = 2, DisabledNumber = 1, AnimalsNumber = 0,
                    Latitude = -29.9197, Longitude = -51.1742,
                    Status = RescueRS.Application.Enums.RescueStatusEnum.Pending,
                    ContactPhone = "51988002222",
                    Description = "Casal de idosos no térreo, um cadeirante. Sem energia elétrica há 2 dias.",
                    RequestedBy = solicitanteIds[1], ConfirmedBy = Guid.Empty, UpdatedBy = solicitanteIds[1],
                },
                new ResgateRS.Domain.Application.Entities.RescueEntity
                {
                    RescueId = Guid.NewGuid(),
                    RequestDateTime = DateTimeOffset.UtcNow.AddHours(-1),
                    AdultsNumber = 5, ChildrenNumber = 3, ElderlyNumber = 0, DisabledNumber = 0, AnimalsNumber = 2,
                    Latitude = -29.9687, Longitude = -51.1153,
                    Status = RescueRS.Application.Enums.RescueStatusEnum.InProgress,
                    ContactPhone = "51988003333",
                    Description = "Duas famílias no telhado com crianças pequenas. Precisam de barco urgente.",
                    RequestedBy = solicitanteIds[2], ConfirmedBy = Guid.Empty, UpdatedBy = rescuerId,
                },
                new ResgateRS.Domain.Application.Entities.RescueEntity
                {
                    RescueId = Guid.NewGuid(),
                    RequestDateTime = DateTimeOffset.UtcNow.AddHours(-5),
                    AdultsNumber = 4, ChildrenNumber = 1, ElderlyNumber = 1, DisabledNumber = 0, AnimalsNumber = 3,
                    Latitude = -30.0872, Longitude = -51.3708,
                    Status = RescueRS.Application.Enums.RescueStatusEnum.Completed,
                    RescueDateTime = DateTimeOffset.UtcNow.AddHours(-4),
                    ContactPhone = "51988004444",
                    Description = "Casa com água no peito. Resgatados com barco. Tinham 3 cachorros.",
                    RequestedBy = solicitanteIds[3], ConfirmedBy = adminId, UpdatedBy = adminId,
                },
                new ResgateRS.Domain.Application.Entities.RescueEntity
                {
                    RescueId = Guid.NewGuid(),
                    RequestDateTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                    AdultsNumber = 1, ChildrenNumber = 0, ElderlyNumber = 0, DisabledNumber = 0, AnimalsNumber = 0,
                    Latitude = -30.0346, Longitude = -51.2177,
                    Status = RescueRS.Application.Enums.RescueStatusEnum.Pending,
                    ContactPhone = "51988005555",
                    Description = "Pessoa sozinha em escritório no 3o andar. Sem mantimentos há 1 dia.",
                    RequestedBy = solicitanteIds[4], ConfirmedBy = Guid.Empty, UpdatedBy = solicitanteIds[4],
                }
            );

            db.SaveChanges();
        }
    }

    app.UseRateLimiter();

    app.UseCors("allowedOrigins");

    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseMiddleware<AuthenticationMiddleware>();

    app.UseMiddleware<PaginationHandlerMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}

