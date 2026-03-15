using System.Diagnostics;

namespace ResgateRS.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation(
                "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "{Method} {Path} failed after {ElapsedMs}ms",
                method, path, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
