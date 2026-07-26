using EventHub.Application.Features.Tickets.Commands.CancelTicket;
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

namespace EventHub.Application.UnitTests.Features.Tickets.Commands.CancelTicket;

public class CancelTicketCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypesRepoMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepoMock;

    private readonly CancelTicketCommandHandler _handler;

    public CancelTicketCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();
        _ticketTypesRepoMock = new Mock<IGenericRepository<TicketType>>();
        _eventsRepoMock = new Mock<IGenericRepository<Event>>();

        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypesRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepoMock.Object);

        _handler = new CancelTicketCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTicketNotFound()
    {
        var ticketId = Guid.NewGuid();
        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Ticket>());

        var command = new CancelTicketCommand(ticketId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotOwnTicket()
    {
        var ticketId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(anotherUserId);

        var ticket = new Ticket
        {
            UserId = ownerId,
            Status = TicketStatus.Active
        };
        SetProperty(ticket, "Id", ticketId);

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Ticket> { ticket });

        var command = new CancelTicketCommand(ticketId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Nemate dozvolu da otkažete ovu kartu.");
    }

    [Theory]
    [InlineData(TicketStatus.Cancelled)]
    [InlineData(TicketStatus.Used)]
    public async Task Handle_ShouldReturnFalse_WhenTicketIsAlreadyCancelledOrUsed(TicketStatus status)
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var ticket = new Ticket
        {
            UserId = userId,
            Status = status
        };
        SetProperty(ticket, "Id", ticketId);

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Ticket> { ticket });

        var command = new CancelTicketCommand(ticketId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEventStartsInLessThan24Hours()
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var ticket = new Ticket
        {
            UserId = userId,
            Status = TicketStatus.Active,
            Event = new Event
            {
                StartDate = DateTime.UtcNow.AddHours(10) 
            }
        };
        SetProperty(ticket, "Id", ticketId);

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Ticket> { ticket });

        var command = new CancelTicketCommand(ticketId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Otkazivanje nije moguće manje od 24 sata pre početka događaja.");
    }

    [Fact]
    public async Task Handle_ShouldCancelTicketSuccessfully_WhenValidRequest()
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var eventEntity = new Event
        {
            StartDate = DateTime.UtcNow.AddDays(2)
        };
        SetProperty(eventEntity, "Status", EventStatus.Published);

        var ticket = new Ticket
        {
            UserId = userId,
            Status = TicketStatus.Active,
            TicketTypeId = ticketTypeId,
            Event = eventEntity
        };
        SetProperty(ticket, "Id", ticketId);

        var ticketType = new TicketType("VIP", 100, 50);
        SetProperty(ticketType, "Id", ticketTypeId);
        
        ticketType.Reserve(1);
        ticketType.ConfirmPurchase(1);

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Ticket> { ticket });

        _ticketTypesRepoMock
            .Setup(r => r.GetByIdAsync(ticketTypeId))
            .ReturnsAsync(ticketType);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new CancelTicketCommand(ticketId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Cancelled);

        _ticketsRepoMock.Verify(r => r.Update(ticket), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    private void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName) ?? obj.GetType().BaseType?.GetProperty(propertyName);
        property?.SetValue(obj, value);
    }
}