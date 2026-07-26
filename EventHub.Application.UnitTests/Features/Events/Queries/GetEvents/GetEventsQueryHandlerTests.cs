using AutoMapper;
using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Queries.GetEvents;

public class GetEventsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IGenericRepository<Event>> _eventRepoMock;

    private readonly GetEventsQueryHandler _handler;

    public GetEventsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _eventRepoMock = new Mock<IGenericRepository<Event>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepoMock.Object);

        _handler = new GetEventsQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredAndPagedEvents_WhenQueryIsValid()
    {
        var categoryId = Guid.NewGuid();

        var event1 = CreateEvent(EventStatus.Published, false, "Concert", categoryId, DateTime.Now.AddDays(1), "Belgrade");
        var event2 = CreateEvent(EventStatus.Draft, false, "Draft Event", categoryId, DateTime.Now.AddDays(2), "Belgrade");
        var event3 = CreateEvent(EventStatus.Published, true, "Private Event", categoryId, DateTime.Now.AddDays(3), "Belgrade");

        var eventsList = new List<Event> { event1, event2, event3 }.AsQueryable();

        _eventRepoMock.Setup(r => r.GetQueryable(It.IsAny<string[]>()))
                      .Returns(eventsList);

        var query = new GetEventsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            CategoryId = categoryId
        };

        var expectedDtos = new List<EventDto>
        {
            new EventDto { Title = "Concert" }
        };

        _mapperMock.Setup(m => m.Map<IEnumerable<EventDto>>(It.IsAny<IEnumerable<Event>>()))
                   .Returns(expectedDtos);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1); 
        result.Items.Should().BeEquivalentTo(expectedDtos);
        _mapperMock.Verify(m => m.Map<IEnumerable<EventDto>>(It.IsAny<IEnumerable<Event>>()), Times.Once);
    }

    private Event CreateEvent(EventStatus status, bool isPrivate, string title, Guid categoryId, DateTime startDate, string location)
    {
        var ev = Activator.CreateInstance<Event>()!;
        typeof(Event).GetProperty("Status")?.SetValue(ev, status);
        typeof(Event).GetProperty("IsPrivate")?.SetValue(ev, isPrivate);
        typeof(Event).GetProperty("Title")?.SetValue(ev, title);
        typeof(Event).GetProperty("CategoryId")?.SetValue(ev, categoryId);
        typeof(Event).GetProperty("StartDate")?.SetValue(ev, startDate);
        typeof(Event).GetProperty("Location")?.SetValue(ev, location);
        return ev;
    }
}