using System;
using System.Threading;

namespace ResgateRS.DTOs;

public static class Response
{
    public static IResponse<T> Fail<T>(string? message, ResultType result = ResultType.Error)
    {
        return Response<T>.Fail(message, result);
    }

    public static IResponse<T> Success<T>(T? data, string? message = "", ResultType result = ResultType.Ok)
    {
        return Response<T>.Success(data, message, result);
    }

    public static IResponse<T> SafeAction<T>(Func<T> action)
    {
        try
        {
            return Success(action());
        }
        catch (Exception ex)
        {
            return Fail<T>(ex.ToString());
        }
    }

    public static IResponse<T> SafeFlatAction<T>(Func<IResponse<T>> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            return Fail<T>(ex.ToString());
        }
    }
    
    public static IResponse<T> ReplacingResult<T>(this IResponse<T> res, ResultType result)
    {
        return Response<T>.Fail(res.Message, result, res.Data, res.DebugMessage);
    }
}