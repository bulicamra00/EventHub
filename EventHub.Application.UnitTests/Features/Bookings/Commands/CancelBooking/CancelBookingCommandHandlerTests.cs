using EventHub.Application.Features.Bookings.Commands.CancelBooking;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Bookings.Commands.CancelBooking;

public class CancelBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Booking>> _bookingRepositoryMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypeRepositoryMock;
    private readonly CancelBookingCommandHandler _handler;

    public CancelBookingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepositoryMock = new Mock<IGenericRepository<Booking>>();
        _ticketTypeRepositoryMock = new Mock<IGenericRepository<TicketType>>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypeRepositoryMock.Object);

        _handler = new CancelBookingCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenBookingExistsAndIsPending_ShouldCancelAndReleaseTickets()
    {
        var bookingId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        
        var booking = new Booking();
        typeof(Booking).GetProperty(nameof(Booking.Id))?.SetValue(booking, bookingId);
        typeof(Booking).GetProperty(nameof(Booking.TicketTypeId))?.SetValue(booking, ticketTypeId);
        typeof(Booking).GetProperty(nameof(Booking.Quantity))?.SetValue(booking, 2);
        typeof(Booking).GetProperty(nameof(Booking.Status))?.SetValue(booking, BookingStatus.Pending);

        var ticketType = new TicketType();
        typeof(TicketType).GetProperty(nameof(TicketType.Id))?.SetValue(ticketType, ticketTypeId);
        
        typeof(TicketType).GetProperty(nameof(TicketType.ReservedCount))?.SetValue(ticketType, 2);

        _bookingRepositoryMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        _ticketTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketTypeId))
            .ReturnsAsync(ticketType);

        var command = new CancelBookingCommand { BookingId = bookingId };

        await _handler.Handle(command, CancellationToken.None);

        booking.Status.Should().Be(BookingStatus.Cancelled);
        _ticketTypeRepositoryMock.Verify(r => r.Update(ticketType), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBookingNotFound_ShouldThrowKeyNotFoundException()
    {
        var bookingId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync((Booking?)null);

        var command = new CancelBookingCommand { BookingId = bookingId };

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never());
    }

    [Fact]
    public async Task Handle_WhenBookingIsNotPending_ShouldThrowInvalidOperationException()
    {
        var bookingId = Guid.NewGuid();
        var booking = new Booking();
        typeof(Booking).GetProperty(nameof(Booking.Id))?.SetValue(booking, bookingId);
        typeof(Booking).GetProperty(nameof(Booking.Status))?.SetValue(booking, BookingStatus.Confirmed);

        _bookingRepositoryMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        var command = new CancelBookingCommand { BookingId = bookingId };

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never());
    }
}