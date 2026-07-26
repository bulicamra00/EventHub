using EventHub.Application.Features.Tickets.Commands.ScanTicket;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Tickets.Commands.ScanTicket;

public class ScanTicketCommandValidatorTests
{
    private readonly ScanTicketCommandValidator _validator;

    public ScanTicketCommandValidatorTests()
    {
        _validator = new ScanTicketCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_TicketCode_Is_Empty(string? ticketCode)
    {
        var command = new ScanTicketCommand(ticketCode!, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TicketCode);
    }

    [Theory]
    [InlineData("short-code")]
    [InlineData("12345678-1234-1234-1234-1234567890123")] 
    public void Should_Have_Error_When_TicketCode_Length_Is_Invalid(string ticketCode)
    {
        var command = new ScanTicketCommand(ticketCode, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TicketCode);
    }

    [Fact]
    public void Should_Have_Error_When_EventId_Is_Empty()
    {
        var validCode = Guid.NewGuid().ToString(); 
        var command = new ScanTicketCommand(validCode, Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EventId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var validCode = Guid.NewGuid().ToString(); 
        var command = new ScanTicketCommand(validCode, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}