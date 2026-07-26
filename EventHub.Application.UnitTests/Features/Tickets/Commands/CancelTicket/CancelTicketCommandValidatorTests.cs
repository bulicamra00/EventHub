using EventHub.Application.Features.Tickets.Commands.CancelTicket;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Tickets.Commands.CancelTicket;

public class CancelTicketCommandValidatorTests
{
    private readonly CancelTicketCommandValidator _validator;

    public CancelTicketCommandValidatorTests()
    {
        _validator = new CancelTicketCommandValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTicketIdIsEmpty()
    {
        var command = new CancelTicketCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.TicketId)
              .WithErrorMessage("ID karte je obavezan.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenTicketIdIsValid()
    {
        var command = new CancelTicketCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.TicketId);
    }
}