using EventHub.Application.Features.Tickets.Commands.PurchaseTicket;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Tickets.Commands.PurchaseTicket;

public class PurchaseTicketCommandValidatorTests
{
    private readonly PurchaseTicketCommandValidator _validator;

    public PurchaseTicketCommandValidatorTests()
    {
        _validator = new PurchaseTicketCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TicketTypeId_Is_Empty()
    {
        var command = new PurchaseTicketCommand(Guid.Empty, 1, "John Doe", "john@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TicketTypeId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Quantity_Is_Less_Than_One(int quantity)
    {
        var command = new PurchaseTicketCommand(Guid.NewGuid(), quantity, "John Doe", "john@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Quantity);
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Exceeds_Maximum()
    {
        var command = new PurchaseTicketCommand(Guid.NewGuid(), 11, "John Doe", "john@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Quantity);
    }

    [Fact]
    public void Should_Have_Error_When_AttendeeName_Is_Empty()
    {
        var command = new PurchaseTicketCommand(Guid.NewGuid(), 1, "", "john@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AttendeeName);
    }

    [Fact]
    public void Should_Have_Error_When_AttendeeEmail_Is_Invalid()
    {
        var command = new PurchaseTicketCommand(Guid.NewGuid(), 1, "John Doe", "invalid-email");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AttendeeEmail);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new PurchaseTicketCommand(Guid.NewGuid(), 2, "John Doe", "john@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}