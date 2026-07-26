using EventHub.Application.Features.Users.Commands.UpdateUser;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator;

    public UpdateUserCommandValidatorTests()
    {
        _validator = new UpdateUserCommandValidator();
    }

    [Theory]
    [InlineData("")]
    public void ShouldHaveError_WhenFullNameIsEmpty(string fullName)
    {
        var command = new UpdateUserCommand(fullName, "Beograd", "Sport");

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void ShouldHaveError_WhenFullNameIsNull()
    {
        var command = new UpdateUserCommand(null!, "Beograd", "Sport");

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void ShouldHaveError_WhenFullNameExceedsMaxLength()
    {
        var longName = new string('a', 101);
        var command = new UpdateUserCommand(longName, "Beograd", "Sport");

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("")]
    public void ShouldHaveError_WhenCityIsEmpty(string city)
    {
        var command = new UpdateUserCommand("Pera Peric", city, "Sport");

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void ShouldHaveError_WhenCityIsNull()
    {
        var command = new UpdateUserCommand("Pera Peric", null!, "Sport");

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void ShouldHaveError_WhenCityExceedsMaxLength()
    {
        var longCity = new string('a', 51);
        var command = new UpdateUserCommand("Pera Peric", longCity, "Sport");

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void ShouldHaveError_WhenInterestsExceedsMaxLength()
    {
        var longInterests = new string('a', 501);
        var command = new UpdateUserCommand("Pera Peric", "Beograd", longInterests);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Interests);
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new UpdateUserCommand("Pera Peric", "Beograd", "Sport");

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
        result.ShouldNotHaveValidationErrorFor(x => x.City);
        result.ShouldNotHaveValidationErrorFor(x => x.Interests);
    }
}