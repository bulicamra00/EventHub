using EventHub.Application.Features.Follows.Queries.GetFollowedOrganizers;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Common;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Follows.Queries.GetFollowedOrganizers;

public class GetFollowedOrganizersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Follow>> _followsRepoMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;

    private readonly GetFollowedOrganizersQueryHandler _handler;

    public GetFollowedOrganizersQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _followsRepoMock = new Mock<IGenericRepository<Follow>>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Follows).Returns(_followsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);

        _handler = new GetFollowedOrganizersQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIdIsNull()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);
        var query = new GetFollowedOrganizersQuery();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserDoesNotFollowAnyone()
    {
        var followerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(followerId);

        _followsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(new List<Follow>());

        var query = new GetFollowedOrganizersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFollowedOrganizers_WhenRelationshipsExist()
    {
        var followerId = Guid.NewGuid();
        var organizerId1 = Guid.NewGuid();
        var organizerId2 = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(followerId);

        var followsList = new List<Follow>
        {
            new Follow { FollowerId = followerId, OrganizerId = organizerId1 },
            new Follow { FollowerId = followerId, OrganizerId = organizerId2 }
        };

        var user1 = CreateUser(organizerId1, "Organizer One", "org1@test.com");
        var user2 = CreateUser(organizerId2, "Organizer Two", "org2@test.com");

        var organizersList = new List<User> { user1, user2 };

        _followsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(followsList);

        _usersRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(organizersList);

        var query = new GetFollowedOrganizersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == organizerId1 && o.FullName == "Organizer One" && o.Email == "org1@test.com");
        result.Should().Contain(o => o.Id == organizerId2 && o.FullName == "Organizer Two" && o.Email == "org2@test.com");
    }

    private User CreateUser(Guid id, string fullName, string email)
    {
        var user = new User
        {
            FullName = fullName,
            Email = email
        };

        var idProperty = typeof(BaseEntity).GetProperty("Id") ?? typeof(User).GetProperty("Id");
        idProperty?.SetValue(user, id);

        return user;
    }
}