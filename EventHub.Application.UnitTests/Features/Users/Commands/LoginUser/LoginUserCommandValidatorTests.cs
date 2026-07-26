using EventHub.Application.Features.Users.Commands.LoginUser;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.LoginUser;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator;

    public LoginUserCommandValidatorTests()
    {
        _validator = new LoginUserCommandValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void ShouldHaveError_WhenEmailIsInvalidOrEmpty(string email)
    {
        var command = new LoginUserCommand(email, "Password123!");

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldHaveError_WhenEmailIsNull()
    {
        var command = new LoginUserCommand(null!, "Password123!");

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldHaveError_WhenPasswordIsEmpty()
    {
        var command = new LoginUserCommand("test@example.com", string.Empty);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ShouldHaveError_WhenPasswordIsNull()
    {
        var command = new LoginUserCommand("test@example.com", null!);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new LoginUserCommand("test@example.com", "Password123!");

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}