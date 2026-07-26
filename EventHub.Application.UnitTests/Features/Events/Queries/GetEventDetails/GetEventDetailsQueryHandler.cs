using AutoMapper;
using EventHub.Application.Features.Events.Queries.GetEventDetails;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Queries.GetEventDetails;

public class GetEventDetailsQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Event>> _eventRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    
    private readonly Mock<IGenericRepository<EventInvitation>> _invitationRepoMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketRepoMock;

    private readonly GetEventDetailsQueryHandler _handler;

    public GetEventDetailsQueryHandlerTests()
    {
        _eventRepoMock = new Mock<IGenericRepository<Event>>();
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _invitationRepoMock = new Mock<IGenericRepository<EventInvitation>>();
        _ticketRepoMock = new Mock<IGenericRepository<Ticket>>();

        _unitOfWorkMock.Setup(u => u.EventInvitations).Returns(_invitationRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketRepoMock.Object);

        _handler = new GetEventDetailsQueryHandler(
            _eventRepoMock.Object,
            _mapperMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEventNotFound()
    {
        var eventId = Guid.NewGuid();
        var query = new GetEventDetailsQuery(eventId, null);

        var eventsList = new List<Event>().AsQueryable();
        _eventRepoMock.Setup(r => r.GetQueryable(It.IsAny<string[]>())).Returns(eventsList);

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
                 .WithMessage($"Event sa ID {eventId} nije pronađen.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenEventIsPrivateAndUserNotAuthorized()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _currentUserServiceMock.Setup(s => s.Email).Returns("test@test.com");

        
        var eventEntity = new Event
        {
            IsPrivate = true,
            OrganizerId = organizerId,
            Title = "Private Event",
            Description = "Desc",
            Location = "Loc"
        };
        
        var queryObj = new GetEventDetailsQuery(eventId, null);

        
        var specificEvent = Activator.CreateInstance<Event>();
        typeof(Event).GetProperty("Id")?.SetValue(specificEvent, eventId);
        specificEvent.IsPrivate = true;
        specificEvent.OrganizerId = organizerId;
        specificEvent.Title = "Private Event";
        specificEvent.Description = "Desc";
        specificEvent.Location = "Loc";

        var eventsList = new List<Event> { specificEvent }.AsQueryable();
        _eventRepoMock.Setup(r => r.GetQueryable(It.IsAny<string[]>())).Returns(eventsList);

        _invitationRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<EventInvitation, bool>>>()))
                           .ReturnsAsync((EventInvitation)null!);

        Func<Task> act = async () => await _handler.Handle(queryObj, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Ovaj događaj je privatan i niste autorizovani da ga vidite.");
    }

    [Fact]
    public async Task Handle_ShouldReturnEventDetailsDto_WhenEventIsPublicAndValid()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var specificEvent = Activator.CreateInstance<Event>();
        typeof(Event).GetProperty("Id")?.SetValue(specificEvent, eventId);
        specificEvent.IsPrivate = false;
        specificEvent.Title = "Public Event";
        specificEvent.Description = "Desc";
        specificEvent.Location = "Loc";

        var eventsList = new List<Event> { specificEvent }.AsQueryable();
        _eventRepoMock.Setup(r => r.GetQueryable(It.IsAny<string[]>())).Returns(eventsList);

        var expectedDto = new EventDetailsDto { Title = "Public Event", UserHasTicket = false };
        _mapperMock.Setup(m => m.Map<EventDetailsDto>(specificEvent)).Returns(expectedDto);

        _ticketRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
                       .ReturnsAsync((Ticket)null!);

        var query = new GetEventDetailsQuery(eventId, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserHasTicket.Should().BeFalse();
        _mapperMock.Verify(m => m.Map<EventDetailsDto>(specificEvent), Times.Once);
    }
}