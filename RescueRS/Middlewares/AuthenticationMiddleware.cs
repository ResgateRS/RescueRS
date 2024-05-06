using System.Text;
using System.Text.Json;
using ResgateRS.DTOs;
using ResgateRS.Tools;
using ResgateRS.Auth;
using Microsoft.AspNetCore.Http.Features;
using ResgateRS.Presenter.Controllers;
using Microsoft.AspNetCore.Mvc;
using ResgateRS.Attributes;

namespace ResgateRS.Middleware;

public class AuthenticationMiddleware
{

    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next) =>
        (_next) = (next);

    public async Task InvokeAsync(HttpContext context, UserSession _userSession)
    {

        string hashLogin = context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "") ?? "";

        EndpointMetadataCollection? metadata = context.Features.Get<IEndpointFeature>()?.Endpoint?.Metadata;

        if (metadata != null)
        {
            bool skipAuthentication = metadata.GetMetadata<SkipAuthenticationAttribute>() != null;
            // bool optionalAuthenticationAttribute = metadata.GetMetadata<OptionalAuthenticationAttribute>() != null;

            if (!skipAuthentication)
            {
                if (!JwtManager.IsValidToken(hashLogin, out UserSession? userSession))
                    throw new MessageException("Login expirado.", ResultType.Error);

                // _userSession.UserIdGuid = userSession!.UserIdGuid;
                _userSession.Cellphone = userSession!.Cellphone;
                _userSession.Rescuer = userSession!.Rescuer;
            }
        }

        await _next(context);

    }

}
