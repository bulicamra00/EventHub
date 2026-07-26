using EventHub.Application.Features.Users.Commands.RequestOrganizer;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.RequestOrganizer;

public class RequestOrganizerCommandValidatorTests
{
    private readonly RequestOrganizerCommandValidator _validator;

    public RequestOrganizerCommandValidatorTests()
    {
        _validator = new RequestOrganizerCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new RequestOrganizerCommand();

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}