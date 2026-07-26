using EventHub.Application.Features.Follows.Commands.UnfollowOrganizer;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Follows.Commands.UnfollowOrganizer;

public class UnfollowOrganizerCommandValidatorTests
{
    private readonly UnfollowOrganizerCommandValidator _validator;

    public UnfollowOrganizerCommandValidatorTests()
    {
        _validator = new UnfollowOrganizerCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_OrganizerId_Is_Empty()
    {
        var command = new UnfollowOrganizerCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrganizerId)
              .WithErrorMessage("ID organizatora je obavezan.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_OrganizerId_Is_Valid()
    {
        var command = new UnfollowOrganizerCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OrganizerId);
    }
}