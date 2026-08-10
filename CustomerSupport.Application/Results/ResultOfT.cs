namespace CustomerSupport.Application.Results;

public sealed class Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        IsSuccess = true;
        Error = Error.None;
        _value = value;
    }

    private Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error == Error.None)
        {
            throw new ArgumentException("A failed result requires an error.", nameof(error));
        }

        IsSuccess = false;
        Error = error;
        _value = default;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);
}
