using System;
using System.Text.Json.Serialization;
using ResgateRS.Attributes;

namespace ResgateRS.Core.DTOs;

public interface IResponse<out T>
{
    [JsonPropertyName("Result")]
    ResultType Result { get; }

    [JsonPropertyName("Message")]
    string? Message { get; }

    [JsonPropertyName("Data")]
    T? Data { get; }

    [JsonPropertyName("DebugMessage")]
    string? DebugMessage { get; }

    IResponse<T2> Convert<T2>(T2? value);

    IResponse<T2> FlatMap<T2>(Func<T?, IResponse<T2>> fcn);

    IResponse<T2> Map<T2>(Func<T?, T2?> fcn);

    IResponse<T> SetSucessMessage(string message);

    IResponse<T> SetErrorMessage(string message);
}