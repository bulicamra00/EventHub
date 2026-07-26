using EventHub.Application.Features.Events.Commands.AcceptInvitation;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.AcceptInvitation;

public class AcceptInvitationCommandValidatorTests
{
    private readonly AcceptInvitationCommandValidator _validator;

    public AcceptInvitationCommandValidatorTests()
    {
        _validator = new AcceptInvitationCommandValidator();
    }

    [Fact]
    public void WhenTokenIsValid_ShouldNotHaveError()
    {
        var command = new AcceptInvitationCommand("valid-token-12345");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhenTokenIsEmpty_ShouldHaveError(string? token)
    {
        var command = new AcceptInvitationCommand(token!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token pozivnice je obavezan.");
    }

    [Fact]
    public void WhenTokenIsTooShort_ShouldHaveError()
    {
        var command = new AcceptInvitationCommand("kratak");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token nije validnog formata.");
    }
}