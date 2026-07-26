using EventHub.Application.Features.Users.Commands.UpdateUser;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);

        _handler = new UpdateUserCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotLoggedIn()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var command = new UpdateUserCommand("Pera Peric", "Beograd", "Sport");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Korisnik nije ulogovan.");

        _usersRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

        var command = new UpdateUserCommand("Pera Peric", "Beograd", "Sport");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Korisnik nije pronađen.");

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserAndComplete_WhenCommandIsValid()
    {
        var userId = Guid.NewGuid();
        var user = new User 
        { 
            FullName = "Stari Naziv", 
            City = "Stari Grad", 
            Interests = "Stara Interesovanja" 
        };
        typeof(User).BaseType?.GetProperty("Id")?.SetValue(user, userId);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var command = new UpdateUserCommand("Pera Peric", "Beograd", "Nova Interesovanja");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        user.FullName.Should().Be("Pera Peric");
        user.City.Should().Be("Beograd");
        user.Interests.Should().Be("Nova Interesovanja");

        _usersRepoMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}