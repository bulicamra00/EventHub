using EventHub.Application.Features.Users.Commands.RefreshToken;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.RefreshToken;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator;

    public RefreshTokenCommandValidatorTests()
    {
        _validator = new RefreshTokenCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenTokenIsNull()
    {
        var command = new RefreshTokenCommand(null!);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Refresh token ne sme biti null.");
    }

    [Fact]
    public void ShouldHaveError_WhenTokenIsEmpty()
    {
        var command = new RefreshTokenCommand(string.Empty);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Refresh token je obavezan.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenTokenIsValid()
    {
        var command = new RefreshTokenCommand("valid-refresh-token-string");

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
}