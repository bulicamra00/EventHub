using EventHub.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandValidatorTests
{
    private readonly MarkNotificationAsReadCommandValidator _validator;

    public MarkNotificationAsReadCommandValidatorTests()
    {
        _validator = new MarkNotificationAsReadCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = new MarkNotificationAsReadCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id)
              .WithErrorMessage("ID notifikacije je obavezan.");
    }

    [Fact]
    public void Should_NotHaveError_WhenIdIsValid()
    {
        var command = new MarkNotificationAsReadCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Id);
    }
}