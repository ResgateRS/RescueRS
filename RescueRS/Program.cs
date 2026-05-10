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
            db.Users.Add(new ResgateRS.Domain.Application.Entities.UserEntity
            {
                UserId = Guid.NewGuid(),
                Cellphone = adminPhone,
                CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash(adminPhone),
                Rescuer = true,
                Admin = true,
            });
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

