using EventHub.Application.Features.Users.Commands.ConfirmEmail;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.ConfirmEmail;

public class ConfirmEmailCommandValidatorTests
{
    private readonly ConfirmEmailCommandValidator _validator;

    public ConfirmEmailCommandValidatorTests()
    {
        _validator = new ConfirmEmailCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenTokenIsNull()
    {
        var command = new ConfirmEmailCommand(null!);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token ne sme biti null.");
    }

    [Fact]
    public void ShouldHaveError_WhenTokenIsEmpty()
    {
        var command = new ConfirmEmailCommand(string.Empty);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token je obavezan.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenTokenIsValid()
    {
        var command = new ConfirmEmailCommand("some-valid-token-string");

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
}