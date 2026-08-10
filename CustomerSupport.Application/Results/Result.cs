namespace CustomerSupport.Application.Results;

public class Result
{
    private Result(bool isSuccess, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
        {
            throw new ArgumentException("The error must match the result state.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}
