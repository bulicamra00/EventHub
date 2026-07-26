using EventHub.Application.Features.Notifications.Queries.GetMyNotifications;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationsRepoMock;

    private readonly GetMyNotificationsQueryHandler _handler;

    public GetMyNotificationsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _notificationsRepoMock = new Mock<IGenericRepository<Notification>>();

        _unitOfWorkMock.Setup(u => u.Notifications).Returns(_notificationsRepoMock.Object);

        _handler = new GetMyNotificationsQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoNotificationsExistForUser()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _notificationsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Notification, bool>>>()))
            .ReturnsAsync(new List<Notification>());

        var query = new GetMyNotificationsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotificationsOrderedByCreatedAtDescending_WhenNotificationsExist()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var olderDate = DateTime.UtcNow.AddHours(-2);
        var newerDate = DateTime.UtcNow.AddHours(-1);

        var olderNotification = new Notification 
        { 
            UserId = userId, 
            Message = "Older notification", 
            IsRead = true 
        };
        SetCreatedAt(olderNotification, olderDate);

        var newerNotification = new Notification 
        { 
            UserId = userId, 
            Message = "Newer notification", 
            IsRead = false 
        };
        SetCreatedAt(newerNotification, newerDate);

        var notificationsList = new List<Notification> { olderNotification, newerNotification };

        _notificationsRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Notification, bool>>>()))
            .ReturnsAsync(notificationsList);

        var query = new GetMyNotificationsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        result[0].Message.Should().Be("Newer notification");
        result[0].IsRead.Should().BeFalse();
        result[0].CreatedAt.Should().Be(newerDate);

        result[1].Message.Should().Be("Older notification");
        result[1].IsRead.Should().BeTrue();
        result[1].CreatedAt.Should().Be(olderDate);
    }

    private void SetCreatedAt(Notification notification, DateTime createdAt)
    {
        var property = notification.GetType().GetProperty("CreatedAt") ?? notification.GetType().BaseType?.GetProperty("CreatedAt");
        property?.SetValue(notification, createdAt);
    }
}