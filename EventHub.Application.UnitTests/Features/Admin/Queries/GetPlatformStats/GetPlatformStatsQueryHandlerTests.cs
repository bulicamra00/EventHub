using EventHub.Application.Features.Admin.Queries.GetPlatformStats;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Queries.GetPlatformStats;

public class GetPlatformStatsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly Mock<IGenericRepository<Event>> _eventRepositoryMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketRepositoryMock;
    private readonly GetPlatformStatsQueryHandler _handler;

    public GetPlatformStatsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();
        _eventRepositoryMock = new Mock<IGenericRepository<Event>>();
        _ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketRepositoryMock.Object);

        _handler = new GetPlatformStatsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenDataExists_ShouldReturnCorrectPlatformStats()
    {
        var users = new List<User>
        {
            new User { Role = UserRole.Organizer },
            new User { Role = UserRole.Attendee },
            new User { Role = UserRole.Attendee }
        };

        var event1 = new Event();
        event1.Publish(); 

        var event2 = new Event(); 

        var event3 = new Event();
        event3.Publish(); 

        var events = new List<Event> { event1, event2, event3 };

        var tickets = new List<Ticket>
        {
            new Ticket { PurchasePrice = 100.0m },
            new Ticket { PurchasePrice = 150.5m },
            new Ticket { PurchasePrice = 50.0m }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        _eventRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(events);

        _ticketRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(tickets);

        var query = new GetPlatformStatsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalUsers.Should().Be(3);
        result.TotalOrganizers.Should().Be(1);
        result.TotalAttendees.Should().Be(2);
        
        result.TotalEvents.Should().Be(3);
        result.PublishedEvents.Should().Be(2);
        
        result.TotalTicketsSold.Should().Be(3);
        result.TotalRevenue.Should().Be(300.5m);

        _userRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _eventRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _ticketRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}