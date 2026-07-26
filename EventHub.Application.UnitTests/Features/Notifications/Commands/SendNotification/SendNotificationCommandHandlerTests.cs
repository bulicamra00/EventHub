using EventHub.Application.Features.Notifications.Commands.SendNotification;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Notifications.Commands.SendNotification;

public class SendNotificationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepoMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationsRepoMock;

    private readonly SendNotificationCommandHandler _handler;

    public SendNotificationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _eventsRepoMock = new Mock<IGenericRepository<Event>>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();
        _notificationsRepoMock = new Mock<IGenericRepository<Notification>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Notifications).Returns(_notificationsRepoMock.Object);

        _handler = new SendNotificationCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEventNotFound()
    {
        var eventId = Guid.NewGuid();
        _eventsRepoMock.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync((Event?)null);

        var command = new SendNotificationCommand(eventId, "Test Subject", "Test Message");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Događaj nije pronađen.");
    }

    [Fact]
    public async Task Handle_ShouldSendEmailsAndCreateNotifications_WhenValidRequest()
    {
        var eventId = Guid.NewGuid();
        var eventEntity = new Event { Title = "Concert" };
        
        var userId = Guid.NewGuid();
        var user = new User { Email = "user@test.com" };
        var ticket = new Ticket 
        { 
            EventId = eventId, 
            UserId = userId, 
            Status = TicketStatus.Active,
            User = user 
        };

        _eventsRepoMock.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(eventEntity);
        _ticketsRepoMock.Setup(r => r.GetListByConditionAsync(
            It.IsAny<Expression<Func<Ticket, bool>>>(), 
            It.IsAny<string>()
        )).ReturnsAsync(new List<Ticket> { ticket });

        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var command = new SendNotificationCommand(eventId, "Important Update", "Show starts at 8 PM.");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();

        _emailServiceMock.Verify(e => e.SendEmailAsync(
            "user@test.com", 
            "Important Update", 
            It.Is<string>(body => body.Contains("Concert") && body.Contains("Show starts at 8 PM."))
        ), Times.Once);

        _notificationsRepoMock.Verify(n => n.AddAsync(It.Is<Notification>(notif => 
            notif.UserId == userId && notif.Message.Contains("Concert")
        )), Times.Once);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}