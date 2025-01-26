using System;
using CSharpMath.Structures;

#pragma warning disable CA1815 // Override equals and operator equals on value types
// Justification for CA1815: Results are not meant to be equated
#pragma warning disable CA2225 // Operator overloads have named alternates
// Justification for CA2225: Use the constructors instead

namespace CSharpMath.Structures;

//For Result<string> where both implicit conversions fight over each other,
//use Err(string) there instead
public readonly struct ResultImplicitError(string error) {
    public string Error { get; } = error ?? throw new ArgumentNullException(nameof(error));
}
public readonly struct Result(string error) {
    public static Result Ok() => new();
    public static Result<T> Ok<T>(T value) => new(value);
    public static SpanResult<T> Ok<T>(ReadOnlySpan<T> value) => new(value);
    public static ResultImplicitError Err(string error) => new(error);
    public string? Error { get; } = error ?? throw new ArgumentNullException(nameof(error));

    public void Match(Action successAction, Action<string> errorAction) {
        if (Error != null) errorAction(Error); else successAction();
    }
    public TResult Match<TResult>(Func<TResult> successFunc, Func<string, TResult> errorFunc) =>
        Error != null ? errorFunc(Error) : successFunc();
    public Result Bind<T>(Action successAction) {
        if (Error != null) return Error;
        successAction(); return Ok();
    }
    public Result<T> Bind<T>(Func<T> successAction) => Error ?? (Result<T>)successAction();
    public Result Bind(Func<Result> successAction) => Error ?? successAction();
    public Result<T> Bind<T>(Func<Result<T>> successAction) => Error ?? successAction();
    public static implicit operator Result(string error) => new(error);
    public static implicit operator Result(ResultImplicitError error) => new(error.Error);
}
public readonly struct Result<T> {
    public Result(T value) => (Value, Error) = (value, null);

    private Result(string error) =>
        (Value, Error) = (default!, error ?? throw new ArgumentNullException(nameof(error)));
    internal readonly T Value;
    public string? Error { get; }
    public void Deconstruct(out T value, out string? error) =>
        (value, error) = (Value, Error);
    public void Match(Action<T> successAction, Action<string> errorAction) {
        if (Error != null) errorAction(Error); else successAction(Value);
    }
    public TResult Match<TResult>(Func<T, TResult> successFunc, Func<string, TResult> errorFunc) =>
        Error != null ? errorFunc(Error) : successFunc(Value);
    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(string error) => new(error);
    public static implicit operator Result<T>(ResultImplicitError error) => new(error.Error);
    public Result Bind(Action<T> method) {
        if (Error is { } error) return error;
        method(Value);
        return Result.Ok();
    }
    public Result Bind(Func<T, Result> method) => Error ?? method(Value);
    public Result<TResult> Bind<TResult>(Func<T, TResult> method) => Error ?? (Result<TResult>)method(Value);
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> method) => Error ?? method(Value);
}
public readonly ref struct SpanResult<T> {
    public SpanResult(ReadOnlySpan<T> value) {
        _value = value;
        Error = null;
    }

    private SpanResult(string error) {
        _value = default;
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }
    private readonly ReadOnlySpan<T> _value;
    public string? Error { get; }
    public void Deconstruct(out ReadOnlySpan<T> value, out string? error) {
        value = _value;
        error = Error;
    }
    public void Match(Action successAction, System.Action<string> errorAction) {
        if (Error != null) errorAction(Error); else successAction(_value);
    }
    public TResult Match<TResult>(Func<TResult> successAction, System.Func<string, TResult> errorAction) =>
        Error != null ? errorAction(Error) : successAction(_value);
    public static implicit operator SpanResult<T>(ReadOnlySpan<T> value) =>
        new(value);
    public static implicit operator SpanResult<T>(string error) =>
        new(error);
    public static implicit operator SpanResult<T>(ResultImplicitError error) =>
        new(error.Error);
    public delegate void Action(ReadOnlySpan<T> result);
    public delegate void Action<TOther>(ReadOnlySpan<T> thisResult, TOther otherResult);
    public delegate TResult Func<TResult>(ReadOnlySpan<T> result);
    public delegate TResult Func<TOther, TResult>(ReadOnlySpan<T> thisResult, TOther otherResult);
    public Result Bind(Action method) {
        if (Error is { } error) return error;
        method(_value);
        return Result.Ok();
    }
    public Result Bind(Func<Result> method) => Error ?? method(_value);
    public Result<TResult> Bind<TResult>(Func<TResult> method) => Error ?? (Result<TResult>)method(_value);
    public Result<TResult> Bind<TResult>(Func<Result<TResult>> method) => Error ?? method(_value);
}