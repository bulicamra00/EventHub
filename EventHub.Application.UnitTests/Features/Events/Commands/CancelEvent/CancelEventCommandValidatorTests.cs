using EventHub.Application.Features.Events.Commands.CancelEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.CancelEvent;

public class CancelEventCommandValidatorTests
{
    private readonly CancelEventCommandValidator _validator;

    public CancelEventCommandValidatorTests()
    {
        _validator = new CancelEventCommandValidator();
    }

    [Fact]
    public void WhenCommandIsValid_ShouldNotHaveError()
    {
        var command = new CancelEventCommand(Guid.NewGuid(), "Validan razlog otkazivanja");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.EventId);
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void WhenEventIdIsEmpty_ShouldHaveError()
    {
        var command = new CancelEventCommand(Guid.Empty, "Validan razlog otkazivanja");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EventId)
              .WithErrorMessage("ID događaja je obavezan.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhenReasonIsEmpty_ShouldHaveError(string? reason)
    {
        var command = new CancelEventCommand(Guid.NewGuid(), reason!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
              .WithErrorMessage("Razlog otkazivanja je obavezan.");
    }

    [Fact]
    public void WhenReasonIsTooLong_ShouldHaveError()
    {
        var longReason = new string('a', 501);
        var command = new CancelEventCommand(Guid.NewGuid(), longReason);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
              .WithErrorMessage("Razlog ne može biti duži od 500 karaktera.");
    }
}