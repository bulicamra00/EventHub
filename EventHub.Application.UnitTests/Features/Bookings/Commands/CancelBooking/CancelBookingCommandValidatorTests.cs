using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentValidation.TestHelper;
using Moq;
using Xunit;
using EventHub.Application.Features.Bookings.Commands.CancelBooking;

namespace EventHub.Application.UnitTests.Features.Bookings.Commands.CancelBooking;

public class CancelBookingCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Booking>> _bookingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CancelBookingCommandValidator _validator;

    public CancelBookingCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepositoryMock = new Mock<IGenericRepository<Booking>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepositoryMock.Object);

        _validator = new CancelBookingCommandValidator(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Validate_WhenBookingIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new CancelBookingCommand { BookingId = Guid.Empty };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.BookingId)
              .WithErrorMessage("ID rezervacije je obavezan.");
    }

    [Fact]
    public async Task Validate_WhenUserIsOwner_ShouldNotHaveValidationError()
    {
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var booking = new Booking();
        typeof(Booking).GetProperty(nameof(Booking.Id))?.SetValue(booking, bookingId);
        typeof(Booking).GetProperty(nameof(Booking.UserId))?.SetValue(booking, userId);

        _bookingRepositoryMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(userId);

        var command = new CancelBookingCommand { BookingId = bookingId };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WhenUserIsNotOwner_ShouldHaveValidationError()
    {
        var bookingId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var booking = new Booking();
        typeof(Booking).GetProperty(nameof(Booking.Id))?.SetValue(booking, bookingId);
        typeof(Booking).GetProperty(nameof(Booking.UserId))?.SetValue(booking, ownerId);

        _bookingRepositoryMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(anotherUserId);

        var command = new CancelBookingCommand { BookingId = bookingId };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("Nemate pravo da otkažete ovu rezervaciju jer niste njen vlasnik.");
    }

    [Fact]
    public async Task Validate_WhenBookingNotFound_ShouldHaveValidationError()
    {
        var bookingId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync((Booking?)null);

        var command = new CancelBookingCommand { BookingId = bookingId };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("Nemate pravo da otkažete ovu rezervaciju jer niste njen vlasnik.");
    }
}