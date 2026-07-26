using EventHub.Application.Features.Reminders.Commands.SendReminders;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Reminders.Commands.SendReminders;

public class SendRemindersCommandValidatorTests
{
    private readonly SendRemindersCommandValidator _validator;

    public SendRemindersCommandValidatorTests()
    {
        _validator = new SendRemindersCommandValidator();
    }

    [Fact]
    public void Validate_ShouldNotHaveAnyErrors_WhenCommandIsInstantiated()
    {
        var command = new SendRemindersCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}