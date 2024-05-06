using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ResgateRS.DTOs;

public class Response<T> : IResponse<T>
{
    private string? _message;

    [JsonPropertyName("Result")]
    public ResultType Result { get; private set; }

    [JsonPropertyName("Message")]
    public virtual string? Message
    {
        get
        {
            return _message;
        }
        private set
        {
            _message = value;
            string text = _message?.ToLower();
            if ((text != null && text.Contains("an error occurred while executing the command definition. see the inner exception for details.")) || (text != null && text.Contains("no http resource was found that matches the request uri")) || (text != null && text.Contains("an unexpected error occurred")) || (text != null && text.Contains("name resolution failed")))
            {
                DebugMessage = value;
                _message = "Desculpe, ocorreu um problema com sua solicitação. Tente novamente em alguns minutos.";
            }
        }
    }

    [JsonPropertyName("DebugMessage")]
    public string? DebugMessage { get; internal set; }

    [JsonPropertyName("Data")]
    public T? Data { get; private set; }

    [JsonIgnore]
    public bool IsSuccess => Result == ResultType.Ok;

    private Response()
    {
        Result = ResultType.Error;
        Message = string.Empty;
    }

    public IResponse<T2> Convert<T2>(T2? value)
    {
        return new Response<T2>
        {
            Data = value,
            Result = Result,
            Message = Message,
            DebugMessage = DebugMessage
        };
    }

    public IResponse<T2> Map<T2>(Func<T?, T2?> fcn)
    {
        return Convert(IsSuccess ? fcn(Data) : default(T2));
    }

    public IResponse<T2> FlatMap<T2>(Func<T?, IResponse<T2>> fcn)
    {
        if (!IsSuccess)
        {
            return Convert(default(T2));
        }

        return fcn(Data);
    }

    public static IResponse<T> Fail(string? message, ResultType result = ResultType.Error, T? data = default(T?), string? debugMessage = null)
    {
        return new Response<T>
        {
            Message = message,
            Result = result,
            Data = data,
            DebugMessage = debugMessage
        };
    }

    public static IResponse<T> Fail(string message, string debugMessage, ResultType result = ResultType.Error, T? data = default(T?))
    {
        return new Response<T>
        {
            Message = message,
            Result = result,
            Data = data,
            DebugMessage = debugMessage
        };
    }

    public static IResponse<T> Success(T? data, string? message = "", ResultType result = ResultType.Ok)
    {
        return new Response<T>
        {
            Data = data,
            Message = message,
            Result = result
        };
    }

    public IResponse<T> SetSucessMessage(string message)
    {
        if (Result == ResultType.Ok)
        {
            Message = message;
        }

        return this;
    }

    public IResponse<T> SetErrorMessage(string message)
    {
        if (Result == ResultType.Error)
        {
            Message = message;
        }

        return this;
    }
}