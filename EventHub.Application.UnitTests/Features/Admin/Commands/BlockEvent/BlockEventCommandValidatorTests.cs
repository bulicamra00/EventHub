using Xunit;
using FluentValidation.TestHelper;
using System;
using EventHub.Application.Features.Admin.Commands.BlockEvent;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.BlockEvent;

public class BlockEventCommandValidatorTests
{
    private readonly BlockEventCommandValidator _validator;

    public BlockEventCommandValidatorTests()
    {
        _validator = new BlockEventCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new BlockEventCommand(Guid.NewGuid(), "Validan razlog blokiranja.");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEventId_ShouldHaveError()
    {
        var command = new BlockEventCommand(Guid.Empty, "Validan razlog.");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EventId);
    }

    [Fact]
    public void Validate_WithEmptyReason_ShouldHaveError()
    {
        var command = new BlockEventCommand(Guid.NewGuid(), "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_WithReasonExceedingMaxLength_ShouldHaveError()
    {
        var longReason = new string('A', 501); 
        var command = new BlockEventCommand(Guid.NewGuid(), longReason);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}