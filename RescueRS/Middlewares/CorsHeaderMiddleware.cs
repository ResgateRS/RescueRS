namespace ResgateRS.Middleware;

public class CorsHeaderMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Add CORS headers for all origins
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        // context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"); // Common methods
        // context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With"); // Common headers

        await _next(context);
    }
}