using System.Text;
using System.Text.Json;
using ResgateRS.Core.DTOs;

namespace ResgateRS.Middleware;

public class ExceptionHandlerMiddleware {

    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(RequestDelegate next) =>
        (_next) = (next);

    public async Task InvokeAsync(HttpContext context) {
        try {
            // Executa antes da request
            await _next(context);
            // Executa depois da request
        }
        catch (Exception ex) {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            switch (ex) {
                case MessageException:
                    await context.Response.WriteAsync(JsonSerializer.Serialize(Response<string>.Fail(ex.Message, ((MessageException)ex).Result ?? ResultType.Error, null, $"{ex.GetCompleteMessage()} -- {ex.StackTrace}")));
                    break;
                default:
                    await context.Response.WriteAsync((JsonSerializer.Serialize(Response<string>.Fail("Desculpe, ocorreu um problema com a sua solicitação, tente novamente mais tarde", ResultType.Error, null, $"{ex.GetCompleteMessage()} -- {ex.StackTrace}"))));
                    break;
            }
        }
    }

}

public class MessageException : System.Exception
{
    public ResultType? Result { get; set; }
    public MessageException() { }
    public MessageException(string message) : base(message) { }
    public MessageException(string message, ResultType resultType) : base(message) {
        Result = resultType;
    }
    public MessageException(string message, System.Exception inner) : base(message, inner) { }
    public MessageException(string message, ResultType resultType, System.Exception inner) : base(message, inner) {
        Result = resultType;
    }
    protected MessageException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public static class ExceptionExtension {

    public static string GetCompleteMessage(this Exception ex) {
        return $"{ex.Message}{ex.AddInnerMessages()}";
    }

    private static string AddInnerMessages(this Exception ex) {
        if (ex.InnerException != null) {
            return $" -> {ex.InnerException.Message}{ex.InnerException.AddInnerMessages()}";
        }
        return $"";
    }

}