using LedgerFlow.Domain.Common;

namespace LedgerFlow.Domain.Tests.Common;

public class ResultTests
{
    private sealed class CustomResult(bool isSuccess, Error error) : Result(isSuccess, error);

    // Verifies that a parameterless Success result is marked as success with no error
    [Fact]
    public void Success_WithoutValue_ShouldReturnSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    // Verifies that a Failure result contains the provided error and is marked as failure
    [Fact]
    public void Failure_WithError_ShouldReturnFailureResult()
    {
        var error = new Error("Invoice.NotEditable", "The invoice cannot be edited");

        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    // Verifies that a generic Success result contains the value and no error
    [Fact]
    public void Success_WithValue_ShouldReturnSuccessfulResultWithValue()
    {
        const string expectedValue = "Invoice001";

        var result = Result.Success(expectedValue);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    // Verifies that accessing the Value of a failure result throws InvalidOperationException
    [Fact]
    public void Value_WhenFailure_ShouldThrowInvalidOperationException()
    {
        var error = new Error("Invoice.NotFound", "The invoice was not found");
        var result = Result.Failure<string>(error);

        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    // Verifies that creating a failure result with Error.None throws InvalidOperationException
    [Fact]
    public void Failure_WithErrorNone_ShouldThrowInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Failure(Error.None));
    }

    // Verifies that creating a successful result with an error throws InvalidOperationException
    [Fact]
    public void Success_WithError_ShouldThrowInvalidOperationException()
    {
        var error = new Error("Unexpected.Error", "Error detail");

        Assert.Throws<InvalidOperationException>(() => new CustomResult(true, error));
    }
}
