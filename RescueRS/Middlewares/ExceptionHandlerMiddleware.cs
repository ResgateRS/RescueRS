using System.Text;
using System.Text.Json;
using ResgateRS.DTOs;

namespace ResgateRS.Middleware;

public class ExceptionHandlerMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        }
        catch (Exception ex) {
            context.Response.ContentType = "application/json";
            switch (ex) {
                case MessageException msgEx:
                    context.Response.StatusCode = msgEx.HttpStatusCode;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = msgEx.Message }));
                    break;
                default:
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Desculpe, ocorreu um problema com a sua solicitação, tente novamente mais tarde" }));
                    break;
            }
        }
    }

}

public class MessageException : System.Exception
{
    public int HttpStatusCode { get; set; } = 400;
    public ResultType? Result { get; set; }
    public MessageException() { }
    public MessageException(string message, int httpStatusCode = 400) : base(message) {
        HttpStatusCode = httpStatusCode;
    }
    public MessageException(string message, ResultType resultType) : base(message) {
        Result = resultType;
        HttpStatusCode = resultType == ResultType.ErrorLogin ? 401 : 400;
    }
    public MessageException(string message, System.Exception inner) : base(message, inner) { }
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