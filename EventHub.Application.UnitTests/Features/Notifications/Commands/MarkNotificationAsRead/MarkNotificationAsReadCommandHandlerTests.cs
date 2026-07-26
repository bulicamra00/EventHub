using EventHub.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationsRepoMock;
    private readonly MarkNotificationAsReadCommandHandler _handler;

    public MarkNotificationAsReadCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationsRepoMock = new Mock<IGenericRepository<Notification>>();

        _unitOfWorkMock.Setup(u => u.Notifications).Returns(_notificationsRepoMock.Object);

        _handler = new MarkNotificationAsReadCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenNotificationNotFound()
    {
        var notificationId = Guid.NewGuid();
        _notificationsRepoMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification?)null);

        var command = new MarkNotificationAsReadCommand(notificationId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMarkAsReadAndReturnTrue_WhenNotificationExists()
    {
        var notificationId = Guid.NewGuid();
        var notification = new Notification 
        { 
            IsRead = false 
        };

        _notificationsRepoMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new MarkNotificationAsReadCommand(notificationId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}