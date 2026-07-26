using EventHub.Application.Features.Admin.Commands.UnblockEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.UnblockEvent;

public class UnblockEventCommandValidatorTests
{
    private readonly UnblockEventCommandValidator _validator;

    public UnblockEventCommandValidatorTests()
    {
        _validator = new UnblockEventCommandValidator();
    }

    [Fact]
    public void Validator_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new UnblockEventCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_WhenEventIdIsEmpty_ShouldHaveError()
    {
        var command = new UnblockEventCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EventId);
    }
}