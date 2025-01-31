using Core;

namespace UserService.Core.Tests;

[TestFixture]
public class ResultTests
{
    private readonly Error _someError = new Error("Code","Some error", ErrorType.Problem); 
    private const int IntValue = 42;
    
    [Test]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.EqualTo(Error.None));
        });
    }

    [Test]
    public void Failure_ShouldCreateFailedResult()
    {
        var result = Result.Failure(_someError);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(_someError));
        });
    }

    [Test]
    public void Success_WithValue_ShouldCreateSuccessfulResultWithValue()
    {
        var result = Result.Success(IntValue);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.EqualTo(IntValue));
        });
    }

    [Test]
    public void Constructor_ShouldThrowArgumentException_WhenInvalidErrorState()
    {
        Assert.Throws<ArgumentException>(() => _ = new Result(true, _someError));
        Assert.Throws<ArgumentException>(() => _ = new Result(false, Error.None));
    }
}