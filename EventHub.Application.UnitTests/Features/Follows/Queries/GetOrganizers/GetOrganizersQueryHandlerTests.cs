using EventHub.Application.Features.Follows.Queries.GetOrganizers;
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

namespace EventHub.Application.UnitTests.Features.Follows.Queries.GetOrganizers;

public class GetOrganizersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepoMock;
    private readonly Mock<IGenericRepository<Follow>> _followsRepoMock;

    private readonly GetOrganizersQueryHandler _handler;

    public GetOrganizersQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _eventsRepoMock = new Mock<IGenericRepository<Event>>();
        _followsRepoMock = new Mock<IGenericRepository<Follow>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Follows).Returns(_followsRepoMock.Object);

        _handler = new GetOrganizersQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrganizersExist()
    {
        _usersRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        _eventsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(new List<Event>());

        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var query = new GetOrganizersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnOrganizersSummary_WhenUserIsNotLoggedIn()
    {
        var organizer = CreateUser("Test Organizer", "org@test.com", UserRole.Organizer);
        var organizerId = organizer.Id;

        _usersRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { organizer });

        var eventsList = new List<Event>
        {
            new Event { OrganizerId = organizerId, IsPrivate = false },
            new Event { OrganizerId = organizerId, IsPrivate = false }
        };

        _eventsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(eventsList);

        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var query = new GetOrganizersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var summary = result[0];
        summary.Id.Should().Be(organizerId);
        summary.FullName.Should().Be("Test Organizer");
        summary.IsFollowed.Should().BeFalse();
        summary.PublishedEventsCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrganizersWithCorrectIsFollowedStatus_WhenUserIsLoggedIn()
    {
        var currentUserId = Guid.NewGuid();

        var organizer1 = CreateUser("Organizer One", "org1@test.com", UserRole.Organizer);
        var organizer2 = CreateUser("Organizer Two", "org2@test.com", UserRole.Organizer);

        _usersRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { organizer1, organizer2 });

        _eventsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(new List<Event>());

        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var followsList = new List<Follow>
        {
            new Follow { FollowerId = currentUserId, OrganizerId = organizer1.Id }
        };

        _followsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(followsList);

        var query = new GetOrganizersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result.Find(o => o.Id == organizer1.Id)!.IsFollowed.Should().BeTrue();
        result.Find(o => o.Id == organizer2.Id)!.IsFollowed.Should().BeFalse();
    }

    private User CreateUser(string fullName, string email, UserRole role)
    {
        return new User
        {
            FullName = fullName,
            Email = email,
            Role = role
        };
    }
}