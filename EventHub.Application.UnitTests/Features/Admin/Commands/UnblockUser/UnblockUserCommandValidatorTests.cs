using EventHub.Application.Features.Admin.Commands.UnblockUser;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.UnblockUser;

public class UnblockUserCommandValidatorTests
{
    private readonly UnblockUserCommandValidator _validator;

    public UnblockUserCommandValidatorTests()
    {
        _validator = new UnblockUserCommandValidator();
    }

    [Fact]
    public void Validator_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new UnblockUserCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_WhenUserIdIsEmpty_ShouldHaveError()
    {
        var command = new UnblockUserCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}