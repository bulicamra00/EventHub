using AutoMapper;
using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Application.Features.Events.Queries.GetOrganizerEvents;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Queries.GetOrganizerEvents;

public class GetOrganizerEventsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Event>> _eventRepoMock;

    private readonly GetOrganizerEventsQueryHandler _handler;

    public GetOrganizerEventsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _eventRepoMock = new Mock<IGenericRepository<Event>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepoMock.Object);

        _handler = new GetOrganizerEventsQueryHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnOrganizerEvents_WhenEventsExist()
    {
        var organizerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(organizerId);

        var event1 = new Event { Title = "Event 1", StartDate = DateTime.Now.AddDays(5) };
        typeof(Event).GetProperty(nameof(Event.OrganizerId))?.SetValue(event1, organizerId);
        typeof(Event).GetProperty(nameof(Event.Status))?.SetValue(event1, EventStatus.Published);

        var event2 = new Event { Title = "Event 2", StartDate = DateTime.Now.AddDays(2) };
        typeof(Event).GetProperty(nameof(Event.OrganizerId))?.SetValue(event2, organizerId);
        typeof(Event).GetProperty(nameof(Event.Status))?.SetValue(event2, EventStatus.Draft);

        var event3 = new Event { Title = "Other Event", StartDate = DateTime.Now.AddDays(10) };
        typeof(Event).GetProperty(nameof(Event.OrganizerId))?.SetValue(event3, Guid.NewGuid());
        typeof(Event).GetProperty(nameof(Event.Status))?.SetValue(event3, EventStatus.Published);

        var eventsList = new List<Event> { event1, event2, event3 };

        _eventRepoMock.Setup(r => r.GetQueryable(It.IsAny<string[]>()))
            .Returns(eventsList.AsQueryable());

        var expectedDtos = new List<EventDto>
        {
            new EventDto { Title = "Event 1" },
            new EventDto { Title = "Event 2" }
        };

        _mapperMock.Setup(m => m.Map<IEnumerable<EventDto>>(It.IsAny<IEnumerable<Event>>()))
            .Returns(expectedDtos);

        var query = new GetOrganizerEventsQuery 
        { 
            PageNumber = 1, 
            PageSize = 10, 
            Status = null 
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().BeEquivalentTo(expectedDtos);
        
        _mapperMock.Verify(m => m.Map<IEnumerable<EventDto>>(It.Is<IEnumerable<Event>>(e => e.Count() == 2)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus_WhenStatusIsProvided()
    {
        var organizerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(organizerId);

        var eventPublished = new Event { Title = "Published Event", StartDate = DateTime.Now };
        typeof(Event).GetProperty(nameof(Event.OrganizerId))?.SetValue(eventPublished, organizerId);
        typeof(Event).GetProperty(nameof(Event.Status))?.SetValue(eventPublished, EventStatus.Published);

        var eventDraft = new Event { Title = "Draft Event", StartDate = DateTime.Now };
        typeof(Event).GetProperty(nameof(Event.OrganizerId))?.SetValue(eventDraft, organizerId);
        typeof(Event).GetProperty(nameof(Event.Status))?.SetValue(eventDraft, EventStatus.Draft);

        var eventsList = new List<Event> { eventPublished, eventDraft };

        _eventRepoMock.Setup(r => r.GetQueryable(It.IsAny<string[]>()))
            .Returns(eventsList.AsQueryable());

        var expectedDtos = new List<EventDto>
        {
            new EventDto { Title = "Published Event" }
        };

        _mapperMock.Setup(m => m.Map<IEnumerable<EventDto>>(It.IsAny<IEnumerable<Event>>()))
            .Returns(expectedDtos);

        var query = new GetOrganizerEventsQuery 
        { 
            PageNumber = 1, 
            PageSize = 10, 
            Status = EventStatus.Published 
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().BeEquivalentTo(expectedDtos);
    }
}