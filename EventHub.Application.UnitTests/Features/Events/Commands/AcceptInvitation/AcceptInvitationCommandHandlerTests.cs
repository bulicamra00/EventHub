using EventHub.Application.Features.Events.Commands.AcceptInvitation;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.AcceptInvitation;

public class AcceptInvitationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<EventInvitation>> _invitationRepositoryMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketRepositoryMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypeRepositoryMock;
    private readonly AcceptInvitationCommandHandler _handler;

    public AcceptInvitationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _invitationRepositoryMock = new Mock<IGenericRepository<EventInvitation>>();
        _ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
        _ticketTypeRepositoryMock = new Mock<IGenericRepository<TicketType>>();

        _unitOfWorkMock.Setup(u => u.EventInvitations).Returns(_invitationRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypeRepositoryMock.Object);

        _handler = new AcceptInvitationCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidInvitationAndUser_ShouldAcceptAndReturnTrue()
    {
        var token = "valid-token-12345";
        var userEmail = "test@example.com";
        Guid? userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();

        var invitation = new EventInvitation();
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.Token))?.SetValue(invitation, token);
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.Email))?.SetValue(invitation, userEmail);
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.EventId))?.SetValue(invitation, eventId);
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.IsUsed))?.SetValue(invitation, false);

        var ticketType = new TicketType();
        typeof(TicketType).GetProperty(nameof(TicketType.Id))?.SetValue(ticketType, ticketTypeId);
        typeof(TicketType).GetProperty(nameof(TicketType.EventId))?.SetValue(ticketType, eventId);

        _currentUserServiceMock.Setup(s => s.Email).Returns(userEmail);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _invitationRepositoryMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<EventInvitation, bool>>>()))
            .ReturnsAsync(invitation);

        _ticketRepositoryMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .ReturnsAsync(new List<Ticket>());

        _ticketTypeRepositoryMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<TicketType, bool>>>()))
            .ReturnsAsync(new List<TicketType> { ticketType });

        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var command = new AcceptInvitationCommand(token);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        invitation.IsUsed.Should().BeTrue();

        _ticketRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Once);
        _invitationRepositoryMock.Verify(r => r.Update(invitation), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvitationNotFoundOrAlreadyUsed_ShouldReturnFalse()
    {
        _invitationRepositoryMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<EventInvitation, bool>>>()))
            .ReturnsAsync((EventInvitation?)null);

        var command = new AcceptInvitationCommand("invalid-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserEmailDoesNotMatchInvitation_ShouldReturnFalse()
    {
        var token = "valid-token-12345";
        var invitation = new EventInvitation();
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.Token))?.SetValue(invitation, token);
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.Email))?.SetValue(invitation, "owner@example.com");
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.IsUsed))?.SetValue(invitation, false);

        _currentUserServiceMock.Setup(s => s.Email).Returns("otheruser@example.com");

        _invitationRepositoryMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<EventInvitation, bool>>>()))
            .ReturnsAsync(invitation);

        var command = new AcceptInvitationCommand(token);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasTicketForEvent_ShouldReturnFalse()
    {
        var token = "valid-token-12345";
        var userEmail = "test@example.com";
        Guid? userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var invitation = new EventInvitation();
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.Token))?.SetValue(invitation, token);
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.Email))?.SetValue(invitation, userEmail);
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.EventId))?.SetValue(invitation, eventId);
        typeof(EventInvitation).GetProperty(nameof(EventInvitation.IsUsed))?.SetValue(invitation, false);

        _currentUserServiceMock.Setup(s => s.Email).Returns(userEmail);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _invitationRepositoryMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<EventInvitation, bool>>>()))
            .ReturnsAsync(invitation);

        _ticketRepositoryMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .ReturnsAsync(new List<Ticket> { new Ticket() });

        var command = new AcceptInvitationCommand(token);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}