using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Application.Features.Follows.Queries.GetOrganizerDetails;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Follows.Queries.GetOrganizerDetails;

public class GetOrganizerDetailsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepoMock;
    private readonly Mock<IGenericRepository<Follow>> _followsRepoMock;

    private readonly GetOrganizerDetailsQueryHandler _handler;

    public GetOrganizerDetailsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _eventsRepoMock = new Mock<IGenericRepository<Event>>();
        _followsRepoMock = new Mock<IGenericRepository<Follow>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Follows).Returns(_followsRepoMock.Object);

        _handler = new GetOrganizerDetailsQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenOrganizerNotFound()
    {
        var organizerId = Guid.NewGuid();
        _usersRepoMock.Setup(r => r.GetByIdAsync(organizerId)).ReturnsAsync((User?)null);

        var query = new GetOrganizerDetailsQuery(organizerId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnOrganizerDetails_WhenUserIsNotLoggedIn()
    {
        var organizerId = Guid.NewGuid();
        var organizer = CreateUser(organizerId, "Test Organizer", "org@test.com");

        _usersRepoMock.Setup(r => r.GetByIdAsync(organizerId)).ReturnsAsync(organizer);
        _eventsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(new List<Event>());

        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var query = new GetOrganizerDetailsQuery(organizerId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(organizerId);
        result.FullName.Should().Be("Test Organizer");
        result.Email.Should().Be("org@test.com");
        result.IsFollowed.Should().BeFalse();
        result.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnOrganizerDetailsWithIsFollowedTrue_WhenUserFollowsOrganizer()
    {
        var organizerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var organizer = CreateUser(organizerId, "Test Organizer", "org@test.com");

        _usersRepoMock.Setup(r => r.GetByIdAsync(organizerId)).ReturnsAsync(organizer);
        _eventsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(new List<Event>());

        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var followsList = new List<Follow>
        {
            new Follow { FollowerId = currentUserId, OrganizerId = organizerId }
        };

        _followsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(followsList);

        var query = new GetOrganizerDetailsQuery(organizerId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsFollowed.Should().BeTrue();
    }

    private User CreateUser(Guid id, string fullName, string email)
    {
        var user = new User
        {
            FullName = fullName,
            Email = email
        };

        var idProperty = user.GetType().GetProperty("Id") ?? user.GetType().BaseType?.GetProperty("Id");
        idProperty?.SetValue(user, id);

        return user;
    }
}