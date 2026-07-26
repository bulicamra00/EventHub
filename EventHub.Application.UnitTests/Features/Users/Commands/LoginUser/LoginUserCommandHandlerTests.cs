using EventHub.Application.Features.Users.Commands.LoginUser;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _jwtServiceMock = new Mock<IJwtService>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);

        _handler = new LoginUserCommandHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User)null!);

        var command = new LoginUserCommand("test@example.com", "Password123!");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Neispravan email ili lozinka.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenPasswordIsIncorrect()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            IsEmailVerified = true
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        var command = new LoginUserCommand("test@example.com", "WrongPassword123!");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Neispravan email ili lozinka.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenEmailIsNotVerified()
    {
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            IsEmailVerified = false
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        var command = new LoginUserCommand("test@example.com", password);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Email nije potvrđen. Molimo vas proverite vaš inbox.");
    }

    [Fact]
    public async Task Handle_ShouldReturnTokens_WhenCredentialsAreValidAndEmailVerified()
    {
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            IsEmailVerified = true
        };

        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(j => j.GenerateAccessToken(user))
            .Returns("access-token-xyz");

        _jwtServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh-token-xyz");

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new LoginUserCommand("test@example.com", password);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token-xyz");
        result.RefreshToken.Should().Be("refresh-token-xyz");

        user.RefreshToken.Should().Be("refresh-token-xyz");
        user.RefreshTokenExpiryTime.Should().BeAfter(DateTime.UtcNow);

        _usersRepoMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}