using EventHub.Application.Features.Users.Queries.GetOrganizerProfile;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Queries.GetOrganizerProfile;

public class GetOrganizerProfileQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepoMock;
    private readonly Mock<IGenericRepository<Follow>> _followsRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetOrganizerProfileQueryHandler _handler;

    public GetOrganizerProfileQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _eventsRepoMock = new Mock<IGenericRepository<Event>>();
        _followsRepoMock = new Mock<IGenericRepository<Follow>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Follows).Returns(_followsRepoMock.Object);

        _handler = new GetOrganizerProfileQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotLoggedIn()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var query = new GetOrganizerProfileQuery();

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

        var query = new GetOrganizerProfileQuery();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Korisnik sa ID-em {userId} nije pronađen.");
    }

    [Fact]
    public async Task Handle_ShouldReturnOrganizerProfileDto_WhenUserIsValid()
    {
        var userId = Guid.NewGuid();
        
        var user = new User
        {
            FullName = "Organizer One",
            Email = "organizer@example.com",
            City = "Beograd"
        };
        typeof(User).BaseType?.GetProperty("Id")?.SetValue(user, userId);
        typeof(User).BaseType?.GetProperty("CreatedAt")?.SetValue(user, DateTime.UtcNow.AddDays(-10));

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var eventsList = new List<Event>
        {
            new Event 
            { 
                Title = "Tech Conference", 
                StartDate = DateTime.UtcNow.AddDays(5), 
                OrganizerId = userId 
            }
        };
        typeof(Event).GetProperty("Status")?.SetValue(eventsList[0], EventStatus.Published);

        _eventsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(eventsList);

        var followerUser = new User
        {
            FullName = "Follower User",
            Email = "follower@example.com"
        };
        typeof(User).BaseType?.GetProperty("Id")?.SetValue(followerUser, Guid.NewGuid());

        var followsList = new List<Follow>
        {
            new Follow
            {
                OrganizerId = userId,
                FollowerId = followerUser.Id,
                Follower = followerUser
            }
        };
        _followsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>(), "Follower"))
            .ReturnsAsync(followsList);

        var query = new GetOrganizerProfileQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FullName.Should().Be("Organizer One");
        result.Email.Should().Be("organizer@example.com");
        result.City.Should().Be("Beograd");
        result.FollowersCount.Should().Be(1);
        result.Followers.Should().HaveCount(1);
        result.Followers[0].FullName.Should().Be("Follower User");
        result.CreatedEvents.Should().HaveCount(1);
        result.CreatedEvents[0].Title.Should().Be("Tech Conference");
    }
}