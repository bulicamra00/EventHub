using EventHub.Application.Features.Events.Commands.PublishEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.PublishEvent;

public class PublishEventCommandValidatorTests
{
    private readonly PublishEventCommandValidator _validator;

    public PublishEventCommandValidatorTests()
    {
        _validator = new PublishEventCommandValidator();
    }

    [Fact]
    public void WhenEventIdIsValid_ShouldNotHaveErrors()
    {
        var command = new PublishEventCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void WhenEventIdIsEmpty_ShouldHaveError()
    {
        var command = new PublishEventCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EventId)
            .WithErrorMessage("Event ID je obavezan.");
    }
}