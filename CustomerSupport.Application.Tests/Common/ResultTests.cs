using CustomerSupport.Application.Common;
using Xunit;

namespace CustomerSupport.Application.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_CreatesFailedResultWithError()
    {
        var error = new Error("support.invalid_state", "The operation is not valid in the current state.");

        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        const string value = "resolved";

        var result = Result<string>.Success(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void GenericFailure_ExposesErrorAndDoesNotExposeValue()
    {
        var error = new Error("support.not_found", "The requested resource was not found.");

        var result = Result<string>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
