
using System.Text.Json.Serialization;
using ResgateRS.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ResgateRS.Presenter.Controllers;

[BaseControllerExceptionFilterAttribute]
[AuthenticationFilterAttribute]
public class BaseController<Service, IServiceProvider> : ControllerBase
{
    protected readonly Service mainService;
    protected readonly IServiceProvider serviceProvider;


    public BaseController(Service service, IServiceProvider serviceProvider)
    {
        this.mainService = service;
        this.serviceProvider = serviceProvider;
    }

}

public class AuthenticationFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var type = context.Controller.GetType();

        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var methodInfo = actionDescriptor?.MethodInfo;

        if (type.GetCustomAttribute<SkipAuthenticationFilterAttribute>() != null)
            base.OnActionExecuting(context);

        else if (methodInfo?.GetCustomAttribute<SkipAuthenticationFilterAttribute>() != null)
            base.OnActionExecuting(context);

        else
        {
            string? hashLogin = context.HttpContext.Request.Headers.Authorization;

            if (hashLogin == null || hashLogin.Trim() == "")
                throw new Exception("Token não informado");

            if (hashLogin.Split(" ").Length <= 1)
                throw new Exception("Tipo de Token não especificado");

            if (!hashLogin.Split(" ")[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Tipo de autenticação inválido");

            if (JwtManager.IsValidToken(hashLogin.Split(" ")[1]))
                base.OnActionExecuting(context);
            else
            {
                var result = new JsonResult("Login inválido ou expirado")
                {
                    StatusCode = 403
                };

                context.Result = result;
            }
        }
    }
}

public class BaseControllerExceptionFilterAttribute : ExceptionFilterAttribute
{

    public override void OnException(ExceptionContext context)
    {
        var message = "";
        var exception = context.Exception;
        while (exception != null)
        {
            message += exception.Message + " - ";
            exception = exception.InnerException;
        }
        var result = new JsonResult(new CustomJsonResult
        {
            Result = 0,
            Message = "Desculpe tente novamente mais tarde",
            DebugMessage = $"Exceptions: {message} StackTrace: {context.Exception.StackTrace?.ToString()}"
        });

        result.StatusCode = 200;
        context.Result = result;
        base.OnException(context);
    }
}

public class CustomJsonResult
{
    [JsonPropertyName("Result")]
    public int Result { get; set; }
    [JsonPropertyName("Message")]
    public string Message { get; set; } = "";
    [JsonPropertyName("DebugMessage")]
    public string DebugMessage { get; set; } = "";
}

public class SkipAuthenticationFilterAttribute : Attribute { }