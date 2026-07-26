using EventHub.Application.Features.Bookings.Commands.CreateBooking;
using FluentValidation.TestHelper;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Bookings.Commands.CreateBooking;

public class CreateBookingCommandValidatorTests
{
    private readonly CreateBookingCommandValidator _validator;

    public CreateBookingCommandValidatorTests()
    {
        _validator = new CreateBookingCommandValidator();
    }

    [Fact]
    public async Task Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), 3);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WhenEventIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new CreateBookingCommand(Guid.Empty, Guid.NewGuid(), 2);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.EventId)
              .WithErrorMessage("Događaj je obavezan.");
    }

    [Fact]
    public async Task Validate_WhenTicketTypeIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.Empty, 2);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.TicketTypeId)
              .WithErrorMessage("Tip karte je obavezan.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WhenQuantityIsLessThanOrEqualToZero_ShouldHaveValidationError(int invalidQuantity)
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), invalidQuantity);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
              .WithErrorMessage("Broj karata mora biti veći od 0.");
    }

    [Fact]
    public async Task Validate_WhenQuantityExceedsMaximumLimit_ShouldHaveValidationError()
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), 11);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
              .WithErrorMessage("Ne možete rezervisati više od 10 karata odjednom.");
    }
}