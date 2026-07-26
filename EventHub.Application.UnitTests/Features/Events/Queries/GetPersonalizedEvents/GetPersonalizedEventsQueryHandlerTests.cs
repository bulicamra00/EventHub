using EventHub.Application.Features.Events.Queries.GetPersonalizedEvents;
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

namespace EventHub.Application.UnitTests.Features.Events.Queries.GetPersonalizedEvents;

public class GetPersonalizedEventsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Follow>> _followsRepoMock;
    private readonly Mock<IGenericRepository<Event>> _eventRepoMock;

    private readonly GetPersonalizedEventsQueryHandler _handler;

    public GetPersonalizedEventsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _followsRepoMock = new Mock<IGenericRepository<Follow>>();
        _eventRepoMock = new Mock<IGenericRepository<Event>>();

        _unitOfWorkMock.Setup(u => u.Follows).Returns(_followsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepoMock.Object);

        _handler = new GetPersonalizedEventsQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIdIsNull()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);
        var query = new GetPersonalizedEventsQuery();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserFollowsNoOrganizers()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _followsRepoMock.Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(new List<Follow>());

        var query = new GetPersonalizedEventsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnPersonalizedEventSummaries_WhenOrganizersAreFollowed()
    {
        var userId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var followsList = new List<Follow>
        {
            new Follow { FollowerId = userId, OrganizerId = organizerId }
        };

        _followsRepoMock.Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Follow, bool>>>()))
            .ReturnsAsync(followsList);

        var event1 = new Event 
        { 
            Title = "Public Event", 
            OrganizerId = organizerId, 
            StartDate = DateTime.Now.AddDays(3), 
            Location = "Belgrade", 
            IsPrivate = false 
        };
        typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(event1, eventId);

        var eventsList = new List<Event> { event1 };

        _eventRepoMock.Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<string[]>()))
            .ReturnsAsync(eventsList);

        var query = new GetPersonalizedEventsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Public Event");
        result[0].Id.Should().Be(eventId);
        result[0].IsPrivate.Should().BeFalse();
    }
}