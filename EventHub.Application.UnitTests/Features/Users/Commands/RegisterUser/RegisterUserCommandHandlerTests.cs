using EventHub.Application.Features.Users.Commands.RegisterUser;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IAppConfig> _appConfigMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _emailServiceMock = new Mock<IEmailService>();
        _appConfigMock = new Mock<IAppConfig>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);

        _appConfigMock.Setup(c => c.FrontendUrl).Returns("https://example.com");

        _handler = new RegisterUserCommandHandler(
            _unitOfWorkMock.Object,
            _emailServiceMock.Object,
            _appConfigMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserWithEmailAlreadyExists()
    {
        var existingUser = new User { Email = "test@example.com" };
        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUser);

        var command = new RegisterUserCommand(
            "test@example.com",
            "Password123!",
            "Pera Peric",
            "Beograd"
        );

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Korisnik sa ovom email adresom već postoji.");

        _usersRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUserAndSendVerificationEmail_WhenCommandIsValid()
    {
        _usersRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User)null!);

        User capturedUser = null!;
        _usersRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new RegisterUserCommand(
            "newuser@example.com",
            "Password123!",
            "Ana Anic",
            "Novi Sad"
        );

        var resultId = await _handler.Handle(command, CancellationToken.None);

        resultId.Should().NotBeEmpty();

        capturedUser.Should().NotBeNull();
        capturedUser.Email.Should().Be("newuser@example.com");
        capturedUser.FullName.Should().Be("Ana Anic");
        capturedUser.City.Should().Be("Novi Sad");
        capturedUser.Role.Should().Be(UserRole.Attendee);
        capturedUser.EmailVerificationToken.Should().NotBeNullOrEmpty();
        capturedUser.EmailVerificationTokenExpiry.Should().BeAfter(DateTime.UtcNow);
        BCrypt.Net.BCrypt.Verify("Password123!", capturedUser.PasswordHash).Should().BeTrue();

        _usersRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);

        _emailServiceMock.Verify(e => e.SendEmailAsync(
            "newuser@example.com",
            "Dobrodošli u EventHub - Potvrdite nalog",
            It.Is<string>(msg => msg.Contains("https://example.com/verify-email?token="))
        ), Times.Once);
    }
}