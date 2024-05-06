using ResgateRS.Extensions;
using Microsoft.AspNetCore.Http.Features;
using ResgateRS.Attributes;
using ResgateRS.Pagination;

namespace ResgateRS.Middleware;

public class PaginationHandlerMiddleware
{

    private readonly RequestDelegate _next;

    public PaginationHandlerMiddleware(RequestDelegate next) =>
        (_next) = (next);

    public async Task InvokeAsync(HttpContext context, PaginationDTO _pagination)
    {
        PaginatedRequestAttribute? pagination = context.Features.Get<IEndpointFeature>()?.Endpoint?.Metadata?.GetMetadata<PaginatedRequestAttribute>();

        if (pagination != null)
        {
            int? pageSize = context.Request.Headers["X-PageSize"].FirstOrDefault()?.TryParseInt32();
            int? page = context.Request.Headers["X-Page"].FirstOrDefault()?.TryParseInt32();
            string? cursor = context.Request.Headers["X-Cursor"].FirstOrDefault();

            if (pageSize != null)
            {
                _pagination.pageSize = pageSize.Value;
            }

            switch (pagination.paginationType)
            {
                case PaginationType.Cursor:
                    if (page != null)
                        throw new Exception("Número de página informada em paginação por cursor.");
                    if (cursor != null)
                    {
                        var converter = System.ComponentModel.TypeDescriptor.GetConverter(pagination.Type);
                        object? result = null;
                        try
                        {
                            result = converter.ConvertFrom(cursor);
                            if (result == null)
                                throw new Exception("result null");
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Cursor inválido, {pagination.Type} esperado.", ex);
                        }
                        _pagination.cursor = result;
                    }
                    break;
                case PaginationType.Offset:
                    if (cursor != null)
                        throw new Exception("Cursor informado em paginação por número de página.");

                    if (page != null)
                        _pagination.page = page.Value;
                    break;
                case PaginationType.SinglePage:
                    if (page != null)
                        throw new Exception("Número de página informado em requisição de página única.");
                    if (cursor != null)
                        throw new Exception("Cursor informado em requisição de página única.");
                    break;
            }
        }

        await _next(context);
    }

}