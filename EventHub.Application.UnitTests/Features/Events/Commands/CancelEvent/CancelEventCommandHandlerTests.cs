using EventHub.Application.Features.Events.Commands.CancelEvent;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.CancelEvent;

public class CancelEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepositoryMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepositoryMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationsRepositoryMock;
    private readonly CancelEventCommandHandler _handler;

    public CancelEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _eventsRepositoryMock = new Mock<IGenericRepository<Event>>();
        _ticketsRepositoryMock = new Mock<IGenericRepository<Ticket>>();
        _notificationsRepositoryMock = new Mock<IGenericRepository<Notification>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Notifications).Returns(_notificationsRepositoryMock.Object);

        _handler = new CancelEventCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEventExistsAndValid_ShouldCancelEventSendEmailsAndSaveNotifications()
    {
        var eventId = Guid.NewGuid();
        var command = new CancelEventCommand(eventId, "Loše vreme");

        var eventEntity = new Event();
        typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(eventEntity, eventId);
        typeof(Event).GetProperty(nameof(Event.Title))?.SetValue(eventEntity, "Test Koncert");

        var userId = Guid.NewGuid();
        var user = new User();
        typeof(User).GetProperty(nameof(User.Id))?.SetValue(user, userId);
        typeof(User).GetProperty(nameof(User.Email))?.SetValue(user, "test@example.com");

        var ticket = new Ticket();
        typeof(Ticket).GetProperty(nameof(Ticket.Id))?.SetValue(ticket, Guid.NewGuid());
        typeof(Ticket).GetProperty(nameof(Ticket.EventId))?.SetValue(ticket, eventId);
        typeof(Ticket).GetProperty(nameof(Ticket.UserId))?.SetValue(ticket, userId);
        typeof(Ticket).GetProperty(nameof(Ticket.User))?.SetValue(ticket, user);
        typeof(Ticket).GetProperty(nameof(Ticket.Status))?.SetValue(ticket, TicketStatus.Active);

        var ticketsList = new List<Ticket> { ticket };

        _eventsRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _ticketsRepositoryMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "User"))
            .ReturnsAsync(ticketsList);

        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();

        _eventsRepositoryMock.Verify(r => r.Update(eventEntity), Times.Once);
        
        _emailServiceMock.Verify(e => e.SendEmailAsync(
            user.Email, 
            "Obaveštenje o otkazivanju događaja", 
            It.Is<string>(body => body.Contains("Loše vreme"))), 
            Times.Once);

        _notificationsRepositoryMock.Verify(n => n.AddAsync(It.Is<Notification>(notif => 
            notif.UserId == userId && notif.Message.Contains("Loše vreme"))), 
            Times.Once);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ShouldThrowException()
    {
        var eventId = Guid.NewGuid();
        var command = new CancelEventCommand(eventId, "Razlog");

        _eventsRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Događaj nije pronađen.");

        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}