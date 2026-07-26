using EventHub.Application.Features.Follows.Commands.FollowOrganizer;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Follows.Commands.FollowOrganizer;

public class FollowOrganizerCommandValidatorTests
{
    private readonly FollowOrganizerCommandValidator _validator;

    public FollowOrganizerCommandValidatorTests()
    {
        _validator = new FollowOrganizerCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_OrganizerId_Is_Empty()
    {
        var command = new FollowOrganizerCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrganizerId)
              .WithErrorMessage("ID organizatora je obavezan.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_OrganizerId_Is_Valid()
    {
        var command = new FollowOrganizerCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OrganizerId);
    }
}