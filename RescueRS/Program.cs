using Microsoft.EntityFrameworkCore;
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
        options.AddPolicy(name: "allOrigins",
                          policy =>
                          {
                              policy.AllowAnyOrigin();
                              policy.AllowAnyHeader();
                              policy.AllowAnyMethod();
                          });
    });
}

void ConfigureServices(IServiceCollection services)
{
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

    services.AddDbContext<ResgateRSDbContext>(optionsBuilder => optionsBuilder.UseOracle(builder.Configuration.GetConnectionString("ResgateRSDbContext")));

    services.RegisterRepositoriesAndServices();

    services.AddScoped<UserSession>();

    services.AddScoped<PaginationDTO>();
}

void Configure(WebApplication app)
{
    app.UseCors("allOrigins");

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

