using Xunit;
using FluentValidation.TestHelper;
using System;
using EventHub.Application.Features.Admin.Commands.ApproveOrganizerRequest;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.ApproveOrganizerRequest;

public class ApproveOrganizerRequestCommandValidatorTests
{
    private readonly ApproveOrganizerRequestCommandValidator _validator;

    public ApproveOrganizerRequestCommandValidatorTests()
    {
        _validator = new ApproveOrganizerRequestCommandValidator();
    }

    [Fact]
    public void Validate_WithValidUserId_ShouldNotHaveError()
    {
        var command = new ApproveOrganizerRequestCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyGuid_ShouldHaveError()
    {
        var command = new ApproveOrganizerRequestCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}