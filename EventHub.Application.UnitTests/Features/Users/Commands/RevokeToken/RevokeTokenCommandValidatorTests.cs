using EventHub.Application.Features.Users.Commands.RevokeToken;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.RevokeToken;

public class RevokeTokenCommandValidatorTests
{
    private readonly RevokeTokenCommandValidator _validator;

    public RevokeTokenCommandValidatorTests()
    {
        _validator = new RevokeTokenCommandValidator();
    }

    [Theory]
    [InlineData("")]
    public void ShouldHaveError_WhenTokenIsEmpty(string token)
    {
        var command = new RevokeTokenCommand(token);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void ShouldHaveError_WhenTokenIsNull()
    {
        var command = new RevokeTokenCommand(null!);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void ShouldNotHaveError_WhenTokenIsValid()
    {
        var command = new RevokeTokenCommand("some-valid-token");
        
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
}