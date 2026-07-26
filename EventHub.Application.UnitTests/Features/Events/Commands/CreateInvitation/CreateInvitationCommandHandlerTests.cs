using EventHub.Application.Features.Events.Commands.CreateInvitation;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.CreateInvitation;

public class CreateInvitationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepositoryMock;
    private readonly Mock<IGenericRepository<EventInvitation>> _invitationsRepositoryMock;
    private readonly CreateInvitationCommandHandler _handler;

    public CreateInvitationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _eventsRepositoryMock = new Mock<IGenericRepository<Event>>();
        _invitationsRepositoryMock = new Mock<IGenericRepository<EventInvitation>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EventInvitations).Returns(_invitationsRepositoryMock.Object);

        _handler = new CreateInvitationCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEventExistsAndRequestIsValid_ShouldCreateInvitationSendEmailAndReturnId()
    {
        var eventId = Guid.NewGuid();
        var command = new CreateInvitationCommand(eventId, "test@example.com");

        var eventEntity = new Event();
        typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(eventEntity, eventId);

        _eventsRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();

        _invitationsRepositoryMock.Verify(r => r.AddAsync(It.Is<EventInvitation>(inv => 
            inv.EventId == eventId && 
            inv.Email == "test@example.com" && 
            !string.IsNullOrEmpty(inv.Token))), 
            Times.Once);

        _emailServiceMock.Verify(e => e.SendEmailAsync(
            "test@example.com", 
            "Pozivnica za događaj", 
            It.Is<string>(body => body.Contains("Pozivnica za događaj"))), 
            Times.Once);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ShouldThrowException()
    {
        var eventId = Guid.NewGuid();
        var command = new CreateInvitationCommand(eventId, "test@example.com");

        _eventsRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Event sa ID-jem {eventId} nije pronađen.");

        _invitationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<EventInvitation>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}