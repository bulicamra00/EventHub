using EventHub.Application.Exceptions;
using EventHub.Application.Features.Admin.Commands.BlockUser;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.BlockUser;

public class BlockUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly BlockUserCommandHandler _handler;

    public BlockUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new BlockUserCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldBlockUserAndCompleteUnitOfWork()
    {
        var user = new User();
        var command = new BlockUserCommand(user.Id, "Kršenje uslova korišćenja");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();

        user.IsBlocked.Should().BeTrue();
        user.BanReason.Should().Be(command.Reason);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var command = new BlockUserCommand(userId, "Neki razlog");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}