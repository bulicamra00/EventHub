using EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Bookings.Commands.CancelExpiredBookings;

public class CancelExpiredBookingsCommandValidatorTests
{
    private readonly CancelExpiredBookingsCommandValidator _validator;

    public CancelExpiredBookingsCommandValidatorTests()
    {
        _validator = new CancelExpiredBookingsCommandValidator();
    }

    [Fact]
    public async Task Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        var command = new CancelExpiredBookingsCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}