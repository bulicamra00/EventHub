using EventHub.Application.Features.Users.Commands.RefreshToken;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _jwtServiceMock = new Mock<IJwtService>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);

        _handler = new RefreshTokenCommandHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFoundByToken()
    {
        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User)null!);

        var command = new RefreshTokenCommand("non-existent-token");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Nevažeći ili istekao refresh token.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenHasExpired()
    {
        var expiredUser = new User
        {
            RefreshToken = "expired-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-10),
            RefreshTokenRevoked = null
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(expiredUser);

        var command = new RefreshTokenCommand("expired-token");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Nevažeći ili istekao refresh token.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenIsRevoked()
    {
        var revokedUser = new User
        {
            RefreshToken = "revoked-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
            RefreshTokenRevoked = DateTime.UtcNow.AddMinutes(-5)
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(revokedUser);

        var command = new RefreshTokenCommand("revoked-token");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Nevažeći ili istekao refresh token.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAndRevokeToken_WhenTokenReuseDetected()
    {

        var compromisedUser = new User
        {
            RefreshToken = "compromised-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
            RefreshTokenRevoked = null,
            ReplacedByToken = "some-new-replacement-token"
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(compromisedUser);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new RefreshTokenCommand("compromised-token");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Sigurnosno upozorenje: Token je kompromitovan.");

        compromisedUser.RefreshToken.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        _usersRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRotateTokenAndReturnNewTokens_WhenTokenIsValid()
    {
        var validUser = new User
        {
            RefreshToken = "valid-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
            RefreshTokenRevoked = null,
            ReplacedByToken = null
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(validUser);

        _jwtServiceMock
            .Setup(j => j.GenerateAccessToken(validUser))
            .Returns("new-access-token");

        _jwtServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("new-refresh-token");

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new RefreshTokenCommand("valid-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");

        validUser.RefreshToken.Should().Be("new-refresh-token");
        validUser.ReplacedByToken.Should().Be("new-refresh-token");
        validUser.RefreshTokenRevoked.Should().NotBeNull();
        validUser.RefreshTokenExpiryTime.Should().BeAfter(DateTime.UtcNow);

        _usersRepoMock.Verify(r => r.Update(validUser), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}