using EventHub.Application.Features.Events.Commands.CreateInvitation;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.CreateInvitation;

public class CreateInvitationCommandValidatorTests
{
    private readonly CreateInvitationCommandValidator _validator;

    public CreateInvitationCommandValidatorTests()
    {
        _validator = new CreateInvitationCommandValidator();
    }

    [Fact]
    public void WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new CreateInvitationCommand(Guid.NewGuid(), "test@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void WhenEventIdIsEmpty_ShouldHaveError()
    {
        var command = new CreateInvitationCommand(Guid.Empty, "test@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EventId)
              .WithErrorMessage("EventId je obavezan.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhenEmailIsEmpty_ShouldHaveError(string? email)
    {
        var command = new CreateInvitationCommand(Guid.NewGuid(), email!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email je obavezan.");
    }

    [Fact]
    public void WhenEmailIsInvalid_ShouldHaveError()
    {
        var command = new CreateInvitationCommand(Guid.NewGuid(), "invalid-email");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Unesite ispravnu email adresu.");
    }
}