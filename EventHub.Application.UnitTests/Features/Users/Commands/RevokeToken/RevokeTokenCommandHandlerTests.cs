using EventHub.Application.Features.Users.Commands.RevokeToken;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.RevokeToken;

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);

        _handler = new RevokeTokenCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTokenNotFound()
    {
        var token = "invalid-token";
        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User)null!);

        var command = new RevokeTokenCommand(token);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Token nije pronađen.");

        _usersRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenTokenAlreadyRevoked()
    {
        var token = "already-revoked-token";
        var existingUser = new User 
        { 
            RefreshToken = token, 
            RefreshTokenRevoked = DateTime.UtcNow.AddDays(-1) 
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUser);

        var command = new RevokeTokenCommand(token);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Token je već opozvan.");

        _usersRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRevokeToken_WhenTokenIsValid()
    {
        var token = "valid-refresh-token";
        var existingUser = new User 
        { 
            RefreshToken = token, 
            RefreshTokenRevoked = null 
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new RevokeTokenCommand(token);

        await _handler.Handle(command, CancellationToken.None);

        existingUser.RefreshTokenRevoked.Should().NotBeNull();
        existingUser.RefreshTokenRevoked.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));

        _usersRepoMock.Verify(r => r.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}