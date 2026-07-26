using EventHub.Application.Features.Notifications.Commands.SendNotification;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Notifications.Commands.SendNotification;

public class SendNotificationCommandValidatorTests
{
    private readonly SendNotificationCommandValidator _validator;

    public SendNotificationCommandValidatorTests()
    {
        _validator = new SendNotificationCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenEventIdIsEmpty()
    {
        var command = new SendNotificationCommand(Guid.Empty, "Subject", "Message");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EventId)
              .WithErrorMessage("ID događaja je obavezan.");
    }

    [Fact]
    public void Should_HaveError_WhenSubjectIsEmpty()
    {
        var command = new SendNotificationCommand(Guid.NewGuid(), "", "Message");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Subject)
              .WithErrorMessage("Naslov obaveštenja je obavezan.");
    }

    [Fact]
    public void Should_HaveError_WhenSubjectExceedsMaxLength()
    {
        var longSubject = new string('A', 151);
        var command = new SendNotificationCommand(Guid.NewGuid(), longSubject, "Message");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Subject)
              .WithErrorMessage("Naslov ne može imati više od 150 karaktera.");
    }

    [Fact]
    public void Should_HaveError_WhenMessageIsEmpty()
    {
        var command = new SendNotificationCommand(Guid.NewGuid(), "Subject", "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Message)
              .WithErrorMessage("Tekst obaveštenja je obavezan.");
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        var command = new SendNotificationCommand(Guid.NewGuid(), "Valid Subject", "Valid Message Text");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}