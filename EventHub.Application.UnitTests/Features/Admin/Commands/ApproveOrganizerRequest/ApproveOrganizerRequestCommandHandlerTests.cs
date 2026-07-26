using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EventHub.Application.Features.Admin.Commands.ApproveOrganizerRequest;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using MediatR;
using FluentAssertions;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.ApproveOrganizerRequest;

public class ApproveOrganizerRequestCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly ApproveOrganizerRequestCommandHandler _handler;

    public ApproveOrganizerRequestCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new ApproveOrganizerRequestCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldApproveOrganizerAndSave()
    {
        var userId = Guid.NewGuid();
        var command = new ApproveOrganizerRequestCommand(userId);

        var existingUser = new User
        {
            IsOrganizerRequested = true,
            OrganizerRequestStatus = "Pending",
            Role = UserRole.Attendee 
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        existingUser.IsOrganizerRequested.Should().BeFalse();
        existingUser.OrganizerRequestStatus.Should().Be("Approved");
        existingUser.Role.Should().Be(UserRole.Organizer);

        _userRepositoryMock.Verify(repo => repo.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldThrowKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var command = new ApproveOrganizerRequestCommand(userId);

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Korisnik sa ID-jem {userId} nije pronađen*");

        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}