using EventHub.Application.Features.Events.Commands.UpdateEvent;
using EventHub.Application.Common;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.UpdateEvent;

public class UpdateEventCommandValidatorTests
{
    private readonly UpdateEventCommandValidator _validator;

    public UpdateEventCommandValidatorTests()
    {
        _validator = new UpdateEventCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = CreateValidCommand(id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID događaja je obavezan.");
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        var command = CreateValidCommand(title: string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Naziv događaja je obavezan.");
    }

    [Fact]
    public void Should_Have_Error_When_Title_Exceeds_Maximum_Length()
    {
        var longTitle = new string('a', 151);
        var command = CreateValidCommand(title: longTitle);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Naziv ne može imati više od 150 karaktera.");
    }

    [Fact]
    public void Should_Have_Error_When_StartDate_Is_In_The_Past()
    {
        var command = CreateValidCommand(startDate: DateTime.Now.AddDays(-1));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("Datum početka mora biti u budućnosti.");
    }

    [Fact]
    public void Should_Have_Error_When_EndDate_Is_Before_StartDate()
    {
        var start = DateTime.Now.AddDays(5);
        var end = DateTime.Now.AddDays(4);
        var command = CreateValidCommand(startDate: start, endDate: end);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("Datum završetka mora biti nakon datuma početka.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private UpdateEventCommand CreateValidCommand(
        Guid? id = null,
        string title = "Valid Title",
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return new UpdateEventCommand(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: "Valid description",
            StartDate: startDate ?? DateTime.Now.AddDays(2),
            EndDate: endDate ?? DateTime.Now.AddDays(3),
            Location: "Belgrade",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new List<string> { "Music" },
            TicketTypes: new List<TicketTypeDto>()
        );
    }
}