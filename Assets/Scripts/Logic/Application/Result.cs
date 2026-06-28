#nullable enable

using System;

public class Result : IResult
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public object? ValueObject { get; protected set; } = null;

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static IResult Success() => new Result(true, "");
    public static IResult Failure(string error) => new Result(false, error);

    public void ThrowIfFailed()
    {
        if (!IsSuccess) throw new InvalidOperationException(Error);
    }
}

public sealed class Result<T> : Result, IResult<T>
{

    private Result(bool isSuccess, string error, T? value) : base(isSuccess, error)
    {
        Value = value;
        ValueObject = value;
    }

    public T? Value { get; }

    public static IResult<T> Success(T value) => new Result<T>(true, "", value);
    public new static IResult<T> Failure(string error) => new Result<T>(false, error, default);
}