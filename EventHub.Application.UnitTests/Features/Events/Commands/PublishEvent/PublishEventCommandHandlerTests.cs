using EventHub.Application.Features.Events.Commands.PublishEvent;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.PublishEvent;

public class PublishEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepositoryMock;
    private readonly PublishEventCommandHandler _handler;

    public PublishEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventsRepositoryMock = new Mock<IGenericRepository<Event>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepositoryMock.Object);

        _handler = new PublishEventCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEventExistsAndNotInPublishedState_ShouldPublishEventUpdateAndReturnTrue()
    {
        var eventId = Guid.NewGuid();
        var command = new PublishEventCommand(eventId);

        var eventEntity = new Event();
        typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(eventEntity, eventId);

        _eventsRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        eventEntity.Status.Should().Be(EventStatus.Published);

        _eventsRepositoryMock.Verify(r => r.Update(eventEntity), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ShouldReturnFalse()
    {
        var eventId = Guid.NewGuid();
        var command = new PublishEventCommand(eventId);

        _eventsRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();

        _eventsRepositoryMock.Verify(r => r.Update(It.IsAny<Event>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEventIsAlreadyPublished_ShouldReturnTrueWithoutUpdating()
    {
        var eventId = Guid.NewGuid();
        var command = new PublishEventCommand(eventId);

        var eventEntity = new Event();
        typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(eventEntity, eventId);
        eventEntity.Publish(); 

        _eventsRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();

        _eventsRepositoryMock.Verify(r => r.Update(It.IsAny<Event>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}