using EventHub.Application.Features.Follows.Commands.UnfollowOrganizer;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Follows.Commands.UnfollowOrganizer;

public class UnfollowOrganizerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Follow>> _followsRepoMock;

    private readonly UnfollowOrganizerCommandHandler _handler;

    public UnfollowOrganizerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _followsRepoMock = new Mock<IGenericRepository<Follow>>();

        _unitOfWorkMock.Setup(u => u.Follows).Returns(_followsRepoMock.Object);

        _handler = new UnfollowOrganizerCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserIdIsNull()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);
        var command = new UnfollowOrganizerCommand(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _followsRepoMock.Verify(r => r.Delete(It.IsAny<Follow>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenFollowRelationshipDoesNotExist()
    {
        var followerId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(followerId);

        _followsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(new List<Follow>());

        var command = new UnfollowOrganizerCommand(organizerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _followsRepoMock.Verify(r => r.Delete(It.IsAny<Follow>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFollowAndComplete_WhenFollowRelationshipExists()
    {
        var followerId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(followerId);

        var existingFollow = new Follow { FollowerId = followerId, OrganizerId = organizerId };
        var followsList = new List<Follow> { existingFollow };

        _followsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(followsList);

        var command = new UnfollowOrganizerCommand(organizerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _followsRepoMock.Verify(r => r.Delete(existingFollow), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}