using EventHub.Application.Features.Events.Commands.CreateEvent;
using EventHub.Application.Common;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.CreateEvent;

public class CreateEventCommandValidatorTests
{
    private readonly CreateEventCommandValidator _validator;

    public CreateEventCommandValidatorTests()
    {
        _validator = new CreateEventCommandValidator();
    }

    [Fact]
    public void WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new CreateEventCommand(
            Title: "Validan Koncert",
            Description: "Opis validnog koncerta koji je dovoljno dug.",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(2),
            Location: "Beograd",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new List<string> { "Muzika", "Koncert" },
            TicketTypes: new List<TicketTypeDto>
            {
                new TicketTypeDto("Regular", 1000, 100)
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhenTitleIsEmpty_ShouldHaveError(string? title)
    {
        var command = new CreateEventCommand(
            Title: title!,
            Description: "Opis validnog koncerta koji je dovoljno dug.",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(2),
            Location: "Beograd",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new List<string>(),
            TicketTypes: new List<TicketTypeDto> { new TicketTypeDto("Regular", 1000, 100) }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void WhenStartDateInPast_ShouldHaveError()
    {
        var command = new CreateEventCommand(
            Title: "Validan Koncert",
            Description: "Opis validnog koncerta koji je dovoljno dug.",
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(1),
            Location: "Beograd",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new List<string>(),
            TicketTypes: new List<TicketTypeDto> { new TicketTypeDto("Regular", 1000, 100) }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void WhenEndDateBeforeStartDate_ShouldHaveError()
    {
        var start = DateTime.UtcNow.AddDays(2);
        var command = new CreateEventCommand(
            Title: "Validan Koncert",
            Description: "Opis validnog koncerta koji je dovoljno dug.",
            StartDate: start,
            EndDate: start.AddHours(-1),
            Location: "Beograd",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new List<string>(),
            TicketTypes: new List<TicketTypeDto> { new TicketTypeDto("Regular", 1000, 100) }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("Datum kraja mora biti nakon datuma početka.");
    }

    [Fact]
    public void WhenTicketTypesIsEmpty_ShouldHaveError()
    {
        var command = new CreateEventCommand(
            Title: "Validan Koncert",
            Description: "Opis validnog koncerta koji je dovoljno dug.",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(2),
            Location: "Beograd",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new List<string>(),
            TicketTypes: new List<TicketTypeDto>()
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TicketTypes)
              .WithErrorMessage("Događaj mora imati bar jedan tip karte.");
    }
}