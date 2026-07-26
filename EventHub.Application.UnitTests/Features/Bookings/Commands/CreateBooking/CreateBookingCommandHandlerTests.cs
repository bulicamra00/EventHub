using EventHub.Application.Features.Bookings.Commands.CreateBooking;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Booking>> _bookingRepositoryMock;
    private readonly Mock<IGenericRepository<Event>> _eventRepositoryMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypeRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateBookingCommandHandler _handler;

    public CreateBookingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepositoryMock = new Mock<IGenericRepository<Booking>>();
        _eventRepositoryMock = new Mock<IGenericRepository<Event>>();
        _ticketTypeRepositoryMock = new Mock<IGenericRepository<TicketType>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypeRepositoryMock.Object);

        _handler = new CreateBookingCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateBookingAndReturnId()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        var quantity = 2;

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var @event = new Event();
        typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(@event, eventId);
        typeof(Event).GetProperty(nameof(Event.Status))?.SetValue(@event, EventStatus.Published);
        typeof(Event).GetProperty(nameof(Event.StartDate))?.SetValue(@event, DateTime.UtcNow.AddDays(5));
        
        var ticketType = new TicketType("Regular", 100, 10m);
        typeof(TicketType).GetProperty(nameof(TicketType.Id))?.SetValue(ticketType, ticketTypeId);
        typeof(TicketType).GetProperty(nameof(TicketType.EventId))?.SetValue(ticketType, eventId);
        
        var ticketTypesList = new List<TicketType> { ticketType };
        typeof(Event).GetProperty(nameof(Event.TicketTypes))?.SetValue(@event, ticketTypesList);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(@event);

        _ticketTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketTypeId))
            .ReturnsAsync(ticketType);

        Booking? capturedBooking = null;
        _bookingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b => capturedBooking = b)
            .Returns(Task.CompletedTask);

        var command = new CreateBookingCommand(eventId, ticketTypeId, quantity);

        var resultId = await _handler.Handle(command, CancellationToken.None);

        resultId.Should().NotBeEmpty();

        capturedBooking.Should().NotBeNull();
        capturedBooking!.UserId.Should().Be(userId);
        capturedBooking.EventId.Should().Be(eventId);
        capturedBooking.TicketTypeId.Should().Be(ticketTypeId);
        capturedBooking.Quantity.Should().Be(quantity);
        capturedBooking.Status.Should().Be(BookingStatus.Pending);

        _bookingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), 2);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}