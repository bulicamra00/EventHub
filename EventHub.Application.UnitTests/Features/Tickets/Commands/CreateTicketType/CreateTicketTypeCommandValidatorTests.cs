using EventHub.Application.Features.Tickets.Commands.CreateTicketType;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Tickets.Commands.CreateTicketType;

public class CreateTicketTypeCommandValidatorTests
{
    private readonly CreateTicketTypeCommandValidator _validator;

    public CreateTicketTypeCommandValidatorTests()
    {
        _validator = new CreateTicketTypeCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateTicketTypeCommand(Guid.NewGuid(), "", 100m, null, null, 10);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        var longName = new string('a', 101);
        var command = new CreateTicketTypeCommand(Guid.NewGuid(), longName, 100m, null, null, 10);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Negative()
    {
        var command = new CreateTicketTypeCommand(Guid.NewGuid(), "Standard", -5m, null, null, 10);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Price_Is_Zero()
    {
        var command = new CreateTicketTypeCommand(Guid.NewGuid(), "Free", 0m, null, null, 10);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void Should_Have_Error_When_Capacity_Is_Less_Than_One()
    {
        var command = new CreateTicketTypeCommand(Guid.NewGuid(), "Standard", 100m, null, null, 0);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Capacity);
    }

    [Fact]
    public void Should_Have_Error_When_EarlyBirdPrice_Is_Greater_Than_Price()
    {
        var command = new CreateTicketTypeCommand(Guid.NewGuid(), "Standard", 100m, 120m, DateTime.UtcNow.AddDays(7), 10);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EarlyBirdPrice);
    }

    [Fact]
    public void Should_Not_Have_Error_When_EarlyBirdPrice_Is_Valid()
    {
        var command = new CreateTicketTypeCommand(Guid.NewGuid(), "Standard", 100m, 80m, DateTime.UtcNow.AddDays(7), 10);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EarlyBirdPrice);
    }
}