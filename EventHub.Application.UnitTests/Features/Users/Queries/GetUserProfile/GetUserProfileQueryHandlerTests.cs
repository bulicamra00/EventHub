using EventHub.Application.Features.Users.Queries.GetUserProfile;
using EventHub.Domain.Common;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<IGenericRepository<Booking>> _bookingsRepoMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetUserProfileQueryHandler _handler;

    public GetUserProfileQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _bookingsRepoMock = new Mock<IGenericRepository<Booking>>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);

        _handler = new GetUserProfileQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotLoggedIn()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var query = new GetUserProfileQuery();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Korisnik nije ulogovan.");

        _usersRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

        var query = new GetUserProfileQuery();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Korisnik sa ID-em {userId} nije pronađen.");
    }

    [Fact]
    public async Task Handle_ShouldReturnUserProfileDto_WhenUserIsValid()
    {
        var userId = Guid.NewGuid();

        var user = new User
        {
            FullName = "Pera Peric",
            Email = "pera@example.com",
            City = "Beograd",
            Interests = "Sport, Muzika, IT"
        };
        
        typeof(User).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?
            .SetValue(user, userId);
        typeof(User).GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?
            .SetValue(user, DateTime.UtcNow.AddDays(-30));

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var bookingsList = new List<Booking>
        {
            new Booking { UserId = userId },
            new Booking { UserId = userId }
        };
        _bookingsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(bookingsList);

        var eventEntity = new Event
        {
            Title = "Concert",
            StartDate = DateTime.UtcNow.AddDays(2)
        };
        typeof(Event).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?
            .SetValue(eventEntity, Guid.NewGuid());

        var ticketsList = new List<Ticket>
        {
            new Ticket
            {
                UserId = userId,
                Event = eventEntity
            }
        };
        typeof(Ticket).GetProperty("Status", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?
            .SetValue(ticketsList[0], TicketStatus.Used);

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Event"))
            .ReturnsAsync(ticketsList);

        var query = new GetUserProfileQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FullName.Should().Be("Pera Peric");
        result.Email.Should().Be("pera@example.com");
        result.City.Should().Be("Beograd");
        result.Interests.Should().BeEquivalentTo(new List<string> { "Sport", "Muzika", "IT" });
        result.TotalBookingsCount.Should().Be(2);
        result.AttendedEvents.Should().HaveCount(1);
        result.AttendedEvents[0].Title.Should().Be("Concert");
    }
}