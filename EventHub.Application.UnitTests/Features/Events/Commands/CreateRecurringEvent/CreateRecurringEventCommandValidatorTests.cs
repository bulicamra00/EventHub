using EventHub.Application.Features.Events.Commands.CreateRecurringEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.CreateRecurringEvent;

public class CreateRecurringEventCommandValidatorTests
{
    private readonly CreateRecurringEventCommandValidator _validator;

    public CreateRecurringEventCommandValidatorTests()
    {
        _validator = new CreateRecurringEventCommandValidator();
    }

    [Fact]
    public void WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new CreateRecurringEventCommand
        {
            Title = "Nedeljni sastanak",
            Description = "Opis sastanka",
            StartDate = DateTime.UtcNow.AddDays(1),
            NumberOfWeeks = 4,
            CategoryId = Guid.NewGuid(),
            Location = "Online"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhenTitleIsEmpty_ShouldHaveError(string? title)
    {
        var command = new CreateRecurringEventCommand
        {
            Title = title!,
            Description = "Opis sastanka",
            StartDate = DateTime.UtcNow.AddDays(1),
            NumberOfWeeks = 4,
            CategoryId = Guid.NewGuid(),
            Location = "Online"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Naslov je obavezan.");
    }

    [Fact]
    public void WhenStartDateInPast_ShouldHaveError()
    {
        var command = new CreateRecurringEventCommand
        {
            Title = "Naslov",
            Description = "Opis",
            StartDate = DateTime.UtcNow.AddDays(-1),
            NumberOfWeeks = 4,
            CategoryId = Guid.NewGuid(),
            Location = "Online"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("Datum ne može biti u prošlosti.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(53)]
    public void WhenNumberOfWeeksIsOutOfRange_ShouldHaveError(int numberOfWeeks)
    {
        var command = new CreateRecurringEventCommand
        {
            Title = "Naslov",
            Description = "Opis",
            StartDate = DateTime.UtcNow.AddDays(1),
            NumberOfWeeks = numberOfWeeks,
            CategoryId = Guid.NewGuid(),
            Location = "Online"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NumberOfWeeks)
              .WithErrorMessage("Broj nedelja mora biti između 1 i 52.");
    }

    [Fact]
    public void WhenCategoryIdIsEmpty_ShouldHaveError()
    {
        var command = new CreateRecurringEventCommand
        {
            Title = "Naslov",
            Description = "Opis",
            StartDate = DateTime.UtcNow.AddDays(1),
            NumberOfWeeks = 4,
            CategoryId = Guid.Empty,
            Location = "Online"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
              .WithErrorMessage("Kategorija je obavezna.");
    }
}