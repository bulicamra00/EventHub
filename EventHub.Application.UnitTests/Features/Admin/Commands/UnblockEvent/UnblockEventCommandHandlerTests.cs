using EventHub.Application.Exceptions;
using EventHub.Application.Features.Admin.Commands.UnblockEvent;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.UnblockEvent;

public class UnblockEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Event>> _eventRepositoryMock;
    private readonly UnblockEventCommandHandler _handler;

    public UnblockEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventRepositoryMock = new Mock<IGenericRepository<Event>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);

        _handler = new UnblockEventCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUnblockEventAndCompleteUnitOfWork()
    {
        var eventEntity = new Event();
        var command = new UnblockEventCommand(eventEntity.Id);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(eventEntity.Id))
            .ReturnsAsync(eventEntity);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ShouldThrowNotFoundException()
    {
        var eventId = Guid.NewGuid();
        var command = new UnblockEventCommand(eventId);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}