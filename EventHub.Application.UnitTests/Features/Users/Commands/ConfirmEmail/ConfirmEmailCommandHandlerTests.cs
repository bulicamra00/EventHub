using EventHub.Application.Features.Users.Commands.ConfirmEmail;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly ConfirmEmailCommandHandler _handler;

    public ConfirmEmailCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);

        _handler = new ConfirmEmailCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserNotFoundByToken()
    {
        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User)null!);

        var command = new ConfirmEmailCommand("invalid-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _usersRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTokenHasExpired()
    {
        var expiredUser = new User
        {
            EmailVerificationToken = "expired-token",
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(-5),
            IsEmailVerified = false
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(expiredUser);

        var command = new ConfirmEmailCommand("expired-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _usersRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldConfirmEmailAndReturnTrue_WhenTokenIsValidAndNotExpired()
    {
        var validUser = new User
        {
            EmailVerificationToken = "valid-token",
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(15),
            IsEmailVerified = false
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(validUser);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new ConfirmEmailCommand("valid-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        validUser.IsEmailVerified.Should().BeTrue();
        validUser.EmailVerificationToken.Should().BeNull();
        validUser.EmailVerificationTokenExpiry.Should().BeNull();

        _usersRepoMock.Verify(r => r.Update(validUser), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}