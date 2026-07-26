using EventHub.Application.Features.Events.Commands.UpdateEvent;
using EventHub.Application.Common;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    
    private readonly Mock<IGenericRepository<Event>> _eventRepoMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypeRepoMock;
    private readonly Mock<IGenericRepository<EventTag>> _eventTagRepoMock;
    private readonly Mock<IGenericRepository<Tag>> _tagRepoMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketRepoMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationRepoMock;

    private readonly UpdateEventCommandHandler _handler;

    public UpdateEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _emailServiceMock = new Mock<IEmailService>();

        _eventRepoMock = new Mock<IGenericRepository<Event>>();
        _ticketTypeRepoMock = new Mock<IGenericRepository<TicketType>>();
        _eventTagRepoMock = new Mock<IGenericRepository<EventTag>>();
        _tagRepoMock = new Mock<IGenericRepository<Tag>>();
        _ticketRepoMock = new Mock<IGenericRepository<Ticket>>();
        _notificationRepoMock = new Mock<IGenericRepository<Notification>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypeRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.EventTags).Returns(_eventTagRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Tags).Returns(_tagRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Notifications).Returns(_notificationRepoMock.Object);

        _handler = new UpdateEventCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _emailServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotOrganizer()
    {
        _currentUserServiceMock.Setup(s => s.Role).Returns("User");
        var command = new UpdateEventCommand(
            Guid.NewGuid(), "T", "D", DateTime.Now, DateTime.Now, 
            "L", null, null, null, null, Guid.NewGuid(), false, new(), new()
        );

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Samo organizatori mogu menjati događaje.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEventNotFound()
    {
        _currentUserServiceMock.Setup(s => s.Role).Returns("Organizer");
        var eventId = Guid.NewGuid();
        
        _eventRepoMock.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync((Event)null!);

        var command = new UpdateEventCommand(
            eventId, "T", "D", DateTime.Now, DateTime.Now, 
            "L", null, null, null, null, Guid.NewGuid(), false, new(), new()
        );

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
                 .WithMessage($"Događaj sa ID {eventId} nije pronađen.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotEventOrganizer()
    {
        _currentUserServiceMock.Setup(s => s.Role).Returns("Organizer");
        var differentUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(differentUserId);

        var eventId = Guid.NewGuid();
        var existingEvent = new Event 
        { 
            OrganizerId = Guid.NewGuid(),
            Title = "Title",
            Description = "Desc",
            Location = "Loc"
        };

        _eventRepoMock.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);

        var command = new UpdateEventCommand(
            eventId, "T", "D", DateTime.Now, DateTime.Now, 
            "L", null, null, null, null, Guid.NewGuid(), false, new(), new()
        );

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Nemate dozvolu da menjate ovaj događaj.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateEventAndSendNotifications_WhenTimeOrLocationChanges()
    {
        var organizerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.Role).Returns("Organizer");
        _currentUserServiceMock.Setup(s => s.UserId).Returns(organizerId);

        var eventId = Guid.NewGuid();
        var existingEvent = new Event 
        { 
            OrganizerId = organizerId,
            Title = "Old Title",
            StartDate = DateTime.Now.AddDays(5),
            Location = "Old Location",
            Description = "Desc"
        };

        _eventRepoMock.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);

        var ticketList = new List<Ticket>
        {
            new Ticket 
            { 
                EventId = eventId, 
                Status = TicketStatus.Active,
                UserId = Guid.NewGuid(),
                User = new User { Email = "test@test.com" }
            }
        };

        _ticketRepoMock.Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string[]>()))
                       .ReturnsAsync(ticketList);

        var command = new UpdateEventCommand(
            Id: eventId,
            Title: "New Title",
            Description: "New Desc",
            StartDate: DateTime.Now.AddDays(10),
            EndDate: DateTime.Now.AddDays(11),
            Location: "Old Location",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new(),
            TicketTypes: new()
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        
        _emailServiceMock.Verify(e => e.SendEmailAsync("test@test.com", "Važna izmena detalja događaja", It.IsAny<string>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}