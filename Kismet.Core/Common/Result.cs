using System.Collections.Generic;

namespace Kismet.Core.Common;

public record Result(bool Success, string? Message = null, IReadOnlyCollection<string>? Errors = null)
{
    public static Result Ok(string? message = null) => new(true, message, Array.Empty<string>());

    public static Result Fail(params string[] errors) =>
        new(false, null, errors.Length == 0 ? new[] { "Unknown error." } : errors);
}

public record Result<T>(bool Success, T? Data, string? Message = null, IReadOnlyCollection<string>? Errors = null)
{
    public static Result<T> Ok(T data, string? message = null) => new(true, data, message, Array.Empty<string>());

    public static Result<T> Fail(params string[] errors) =>
        new(false, default, null, errors.Length == 0 ? new[] { "Unknown error." } : errors);
}

