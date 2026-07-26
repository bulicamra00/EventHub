using EventHub.Application.Features.Reminders.Commands.SendReminders;
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

namespace EventHub.Application.UnitTests.Features.Reminders.Commands.SendReminders;

public class SendRemindersCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;

    private readonly SendRemindersCommandHandler _handler;

    public SendRemindersCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();

        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);

        _handler = new SendRemindersCommandHandler(
            _unitOfWorkMock.Object,
            _emailServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldNotSendEmailOrSaveChanges_WhenNoTicketsFound()
    {
        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(new List<Ticket>());

        var command = new SendRemindersCommand();

        await _handler.Handle(command, CancellationToken.None);

        _emailServiceMock.Verify(s => s.SendEmailAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>()
        ), Times.Never);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSendEmailsAndUpdateTickets_WhenMatchingTicketsExist()
    {
        var ticket = new Ticket
        {
            AttendeeEmail = "user@example.com",
            AttendeeName = "Petar Petrovic",
            ReminderSent = false,
            Event = new Event
            {
                Title = "Tech Conference",
                StartDate = DateTime.UtcNow.AddHours(24)
            }
        };

        var ticketsList = new List<Ticket> { ticket };

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.Is<string>(s => s == "Event")))
            .ReturnsAsync(ticketsList);

        var command = new SendRemindersCommand();

        await _handler.Handle(command, CancellationToken.None);

        ticket.ReminderSent.Should().BeTrue();
        ticket.ReminderSentAt.Should().NotBeNull();
        ticket.ReminderSentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _emailServiceMock.Verify(s => s.SendEmailAsync(
            "user@example.com",
            "Podsetnik: Tech Conference počinje za 24h!",
            It.Is<string>(body => body.Contains("Petar Petrovic"))
        ), Times.Once);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipSendingEmail_WhenTicketEventIsNull()
    {
        var ticketWithNullEvent = new Ticket
        {
            AttendeeEmail = "user@example.com",
            AttendeeName = "Petar Petrovic",
            ReminderSent = false,
            Event = null!
        };

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(new List<Ticket> { ticketWithNullEvent });

        var command = new SendRemindersCommand();

        await _handler.Handle(command, CancellationToken.None);

        ticketWithNullEvent.ReminderSent.Should().BeFalse();
        _emailServiceMock.Verify(s => s.SendEmailAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>()
        ), Times.Never);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCatchExceptionAndContinue_WhenEmailSendingFails()
    {
        var ticket = new Ticket
        {
            AttendeeEmail = "user@example.com",
            AttendeeName = "Petar Petrovic",
            ReminderSent = false,
            Event = new Event
            {
                Title = "Tech Conference",
                StartDate = DateTime.UtcNow.AddHours(24)
            }
        };

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(new List<Ticket> { ticket });

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var command = new SendRemindersCommand();

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
        ticket.ReminderSent.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}