using EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Bookings.Commands.CancelExpiredBookings;

public class CancelExpiredBookingsCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Booking>> _bookingRepositoryMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypeRepositoryMock;
    private readonly CancelExpiredBookingsCommandHandler _handler;

    public CancelExpiredBookingsCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepositoryMock = new Mock<IGenericRepository<Booking>>();
        _ticketTypeRepositoryMock = new Mock<IGenericRepository<TicketType>>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypeRepositoryMock.Object);

        _handler = new CancelExpiredBookingsCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenExpiredBookingsExist_ShouldExpireBookingsAndReleaseTickets()
    {
        var bookingId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();

        var booking = new Booking();
        typeof(Booking).GetProperty(nameof(Booking.Id))?.SetValue(booking, bookingId);
        typeof(Booking).GetProperty(nameof(Booking.TicketTypeId))?.SetValue(booking, ticketTypeId);
        typeof(Booking).GetProperty(nameof(Booking.Quantity))?.SetValue(booking, 2);
        typeof(Booking).GetProperty(nameof(Booking.Status))?.SetValue(booking, BookingStatus.Pending);
        typeof(Booking).GetProperty(nameof(Booking.ExpiresAt))?.SetValue(booking, DateTime.UtcNow.AddMinutes(-10));

        var ticketType = new TicketType();
        typeof(TicketType).GetProperty(nameof(TicketType.Id))?.SetValue(ticketType, ticketTypeId);
        typeof(TicketType).GetProperty(nameof(TicketType.ReservedCount))?.SetValue(ticketType, 2);

        var expiredBookingsList = new List<Booking> { booking };

        _bookingRepositoryMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(expiredBookingsList);

        _ticketTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketTypeId))
            .ReturnsAsync(ticketType);

        var command = new CancelExpiredBookingsCommand();

        await _handler.Handle(command, CancellationToken.None);

        booking.Status.Should().Be(BookingStatus.Expired);
        _ticketTypeRepositoryMock.Verify(r => r.Update(ticketType), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoExpiredBookingsExist_ShouldNotCompleteTransaction()
    {
        _bookingRepositoryMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(new List<Booking>());

        var command = new CancelExpiredBookingsCommand();

        await _handler.Handle(command, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}