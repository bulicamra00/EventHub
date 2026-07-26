using EventHub.Application.Features.Users.Commands.RegisterUser;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void ShouldHaveError_WhenEmailIsInvalidOrEmpty(string email)
    {
        var command = new RegisterUserCommand(
            email,
            "Password123!",
            "Pera Peric",
            "Beograd"
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldHaveError_WhenEmailIsNull()
    {
        var command = new RegisterUserCommand(
            null!,
            "Password123!",
            "Pera Peric",
            "Beograd"
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")] 
    public void ShouldHaveError_WhenPasswordIsInvalidOrTooShort(string password)
    {
        var command = new RegisterUserCommand(
            "test@example.com",
            password,
            "Pera Peric",
            "Beograd"
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ShouldHaveError_WhenPasswordIsNull()
    {
        var command = new RegisterUserCommand(
            "test@example.com",
            null!,
            "Pera Peric",
            "Beograd"
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("")]
    public void ShouldHaveError_WhenFullNameIsEmpty(string fullName)
    {
        var command = new RegisterUserCommand(
            "test@example.com",
            "Password123!",
            fullName,
            "Beograd"
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void ShouldHaveError_WhenFullNameIsNull()
    {
        var command = new RegisterUserCommand(
            "test@example.com",
            "Password123!",
            null!,
            "Beograd"
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new RegisterUserCommand(
            "test@example.com",
            "Password123!",
            "Pera Peric",
            "Beograd"
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }
}