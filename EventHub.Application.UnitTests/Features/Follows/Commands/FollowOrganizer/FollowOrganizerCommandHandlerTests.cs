using EventHub.Application.Features.Follows.Commands.FollowOrganizer;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Follows.Commands;

public class FollowOrganizerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Follow>> _followsRepoMock;

    private readonly FollowOrganizerCommandHandler _handler;

    public FollowOrganizerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _followsRepoMock = new Mock<IGenericRepository<Follow>>();

        _unitOfWorkMock.Setup(u => u.Follows).Returns(_followsRepoMock.Object);

        _handler = new FollowOrganizerCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIdIsNull()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);
        var command = new FollowOrganizerCommand(Guid.NewGuid());

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Morate biti ulogovani da biste pratili organizatora.");
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenAlreadyFollowing()
    {
        var followerId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(followerId);

        var existingFollow = new Follow { FollowerId = followerId, OrganizerId = organizerId };

        _followsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(existingFollow);

        var command = new FollowOrganizerCommand(organizerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _followsRepoMock.Verify(r => r.AddAsync(It.IsAny<Follow>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAddFollowAndComplete_WhenNotAlreadyFollowing()
    {
        var followerId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(followerId);

        _followsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync((Follow)null!);

        var command = new FollowOrganizerCommand(organizerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _followsRepoMock.Verify(r => r.AddAsync(It.Is<Follow>(f => f.FollowerId == followerId && f.OrganizerId == organizerId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}