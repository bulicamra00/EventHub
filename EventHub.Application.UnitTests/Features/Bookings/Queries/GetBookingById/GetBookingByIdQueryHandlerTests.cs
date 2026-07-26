using EventHub.Application.Features.Bookings.Queries.GetBookingById;
using EventHub.Application.Features.Bookings.Queries.GetMyBookings;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Bookings.Queries.GetBookingById;

public class GetBookingByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Booking>> _bookingRepositoryMock;
    private readonly GetBookingByIdQueryHandler _handler;

    public GetBookingByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepositoryMock = new Mock<IGenericRepository<Booking>>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepositoryMock.Object);

        _handler = new GetBookingByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenBookingExists_ShouldReturnBookingDto()
    {
        var bookingId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var @event = new Event();
        typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(@event, eventId);
        typeof(Event).GetProperty(nameof(Event.Title))?.SetValue(@event, "Rock Koncert");

        var booking = new Booking();
        typeof(Booking).GetProperty(nameof(Booking.Id))?.SetValue(booking, bookingId);
        typeof(Booking).GetProperty(nameof(Booking.TicketTypeId))?.SetValue(booking, ticketTypeId);
        typeof(Booking).GetProperty(nameof(Booking.EventId))?.SetValue(booking, eventId);
        typeof(Booking).GetProperty(nameof(Booking.Event))?.SetValue(booking, @event);
        typeof(Booking).GetProperty(nameof(Booking.Quantity))?.SetValue(booking, 2);
        typeof(Booking).GetProperty(nameof(Booking.TotalPrice))?.SetValue(booking, 50m);
        typeof(Booking).GetProperty(nameof(Booking.Status))?.SetValue(booking, BookingStatus.Pending);
        typeof(Booking).GetProperty(nameof(Booking.CreatedAt))?.SetValue(booking, DateTime.UtcNow);

        var bookingsList = new List<Booking> { booking };

        _bookingRepositoryMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Booking, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync(bookingsList);

        var query = new GetBookingByIdQuery(bookingId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(bookingId);
        result.EventTitle.Should().Be("Rock Koncert");
        result.TicketTypeId.Should().Be(ticketTypeId);
        result.Quantity.Should().Be(2);
        result.TotalPrice.Should().Be(50m);
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_WhenBookingDoesNotExist_ShouldReturnNull()
    {
        var bookingId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Booking, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync(new List<Booking>());

        var query = new GetBookingByIdQuery(bookingId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}