using AutoMapper;
using EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;
using EventHub.Application.Features.Bookings.Queries.GetMyBookings;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Bookings.Queries.GetMyBookings;

public class GetMyBookingsQueryHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Booking>> _bookingRepositoryMock;
    private readonly GetMyBookingsQueryHandler _handler;

    public GetMyBookingsQueryHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepositoryMock = new Mock<IGenericRepository<Booking>>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepositoryMock.Object);

        _handler = new GetMyBookingsQueryHandler(
            _mediatorMock.Object, 
            _mapperMock.Object, 
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenBookingsExist_ShouldReturnListOfBookingDtos()
    {
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var @event = new Event();
        typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(@event, eventId);
        typeof(Event).GetProperty(nameof(Event.Title))?.SetValue(@event, "Tehno žurka");

        var booking = new Booking();
        typeof(Booking).GetProperty(nameof(Booking.Id))?.SetValue(booking, bookingId);
        typeof(Booking).GetProperty(nameof(Booking.UserId))?.SetValue(booking, userId);
        typeof(Booking).GetProperty(nameof(Booking.EventId))?.SetValue(booking, eventId);
        typeof(Booking).GetProperty(nameof(Booking.Event))?.SetValue(booking, @event);
        typeof(Booking).GetProperty(nameof(Booking.Quantity))?.SetValue(booking, 1);
        typeof(Booking).GetProperty(nameof(Booking.TotalPrice))?.SetValue(booking, 1500m);
        typeof(Booking).GetProperty(nameof(Booking.Status))?.SetValue(booking, BookingStatus.Pending);
        typeof(Booking).GetProperty(nameof(Booking.CreatedAt))?.SetValue(booking, DateTime.UtcNow);

        var bookingsList = new List<Booking> { booking };

        var bookingDto = new BookingDto
        {
            Id = bookingId,
            EventTitle = "Tehno žurka",
            Quantity = 1,
            TotalPrice = 1500m,
            Status = "Pending"
        };

        var expectedDtos = new List<BookingDto> { bookingDto };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelExpiredBookingsCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _bookingRepositoryMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Booking, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync(bookingsList);

        _mapperMock
            .Setup(m => m.Map<List<BookingDto>>(bookingsList))
            .Returns(expectedDtos);

        var query = new GetMyBookingsQuery(userId.ToString());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(bookingId);
        result[0].EventTitle.Should().Be("Tehno žurka");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<CancelExpiredBookingsCommand>(), It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }
}