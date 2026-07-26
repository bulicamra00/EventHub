using EventHub.Application.Features.Admin.Commands.BlockUser;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.BlockUser;

public class BlockUserCommandValidatorTests
{
    private readonly BlockUserCommandValidator _validator;

    public BlockUserCommandValidatorTests()
    {
        _validator = new BlockUserCommandValidator();
    }

    [Fact]
    public void Validator_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new BlockUserCommand(Guid.NewGuid(), "Neprimereno ponašanje na platformi.");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_WhenUserIdIsEmpty_ShouldHaveError()
    {
        var command = new BlockUserCommand(Guid.Empty, "Neprimereno ponašanje.");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234")] 
    public void Validator_WhenReasonIsInvalid_ShouldHaveError(string reason)
    {
        var command = new BlockUserCommand(Guid.NewGuid(), reason);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validator_WhenReasonExceedsMaximumLength_ShouldHaveError()
    {
        var longReason = new string('a', 501); 
        var command = new BlockUserCommand(Guid.NewGuid(), longReason);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}