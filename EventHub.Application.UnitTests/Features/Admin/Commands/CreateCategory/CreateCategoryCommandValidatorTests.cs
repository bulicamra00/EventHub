using EventHub.Application.Features.Admin.Commands.CreateCategory;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.CreateCategory;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator;

    public CreateCategoryCommandValidatorTests()
    {
        _validator = new CreateCategoryCommandValidator();
    }

    [Fact]
    public void Validator_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new CreateCategoryCommand("Tehnologija", "Događaji vezani za IT i programiranje.");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_WhenNameIsEmpty_ShouldHaveError(string name)
    {
        var command = new CreateCategoryCommand(name, "Neki opis");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_WhenNameExceedsMaximumLength_ShouldHaveError()
    {
        var longName = new string('a', 101); 
        var command = new CreateCategoryCommand(longName, "Neki opis");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_WhenDescriptionExceedsMaximumLength_ShouldHaveError()
    {
        var longDescription = new string('a', 501); 
        var command = new CreateCategoryCommand("Kategorija", longDescription);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}