#nullable enable


public interface IResult
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public object? ValueObject { get; }
}

public interface IResult<T> : IResult
{
    public T? Value { get; }
}