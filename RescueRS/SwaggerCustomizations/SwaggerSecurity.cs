using Microsoft.OpenApi.Models;

namespace ResgateRS.SwaggerCustomizations;
public class SwaggerSecurity
{
    public static OpenApiSecurityScheme SecurityScheme => new()
    {
        Description = @"JWT Authorization header using the Bearer scheme. <BR/><BR/> 
                            Enter 'Bearer' [space] and then your token in the text input below.
                            <BR/><BR/>Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    };

    public static OpenApiSecurityRequirement SecurityRequirement => new()
    {
                {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,

                    },
                    new List<string>()
                }
        };
}