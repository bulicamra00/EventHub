using EventHub.Application.Features.Events.Commands.CreateEvent;
using EventHub.Application.Common;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateEventCommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepositoryMock;
    private readonly Mock<IGenericRepository<Tag>> _tagsRepositoryMock;
    private readonly Mock<IGenericRepository<EventTag>> _eventTagsRepositoryMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypesRepositoryMock;
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateEventCommandHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _eventsRepositoryMock = new Mock<IGenericRepository<Event>>();
        _tagsRepositoryMock = new Mock<IGenericRepository<Tag>>();
        _eventTagsRepositoryMock = new Mock<IGenericRepository<EventTag>>();
        _ticketTypesRepositoryMock = new Mock<IGenericRepository<TicketType>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Tags).Returns(_tagsRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EventTags).Returns(_eventTagsRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypesRepositoryMock.Object);

        _handler = new CreateEventCommandHandler(
            _unitOfWorkMock.Object, 
            _loggerMock.Object, 
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsOrganizerAndRequestIsValid_ShouldCreateEventAndReturnId()
    {
        var organizerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.Role).Returns("Organizer");
        _currentUserServiceMock.Setup(s => s.UserId).Returns(organizerId);

        var command = new CreateEventCommand(
            Title: "Test Koncert",
            Description: "Opis",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(2),
            Location: "Beograd",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new List<string> { "Muzika" },
            TicketTypes: new List<TicketTypeDto>
            {
                new TicketTypeDto("VIP", 2000, 50)
            }
        );

        _tagsRepositoryMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Tag, bool>>>()))
            .ReturnsAsync((Tag?)null);

        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();

        _eventsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Once);
        _tagsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Tag>()), Times.Once);
        _eventTagsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<EventTag>()), Times.Once);
        _ticketTypesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketType>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotOrganizer_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.Role).Returns("Attendee");

        var command = new CreateEventCommand(
            Title: "Test Koncert",
            Description: "Opis",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(2),
            Location: "Beograd",
            Latitude: null,
            Longitude: null,
            OnlineLink: null,
            CoverImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsPrivate: false,
            TagNames: new List<string>(),
            TicketTypes: new List<TicketTypeDto> { new TicketTypeDto("VIP", 2000, 50) }
        );

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Samo organizatori mogu kreirati događaje.");

        _eventsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }
}