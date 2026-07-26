using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using EventHub.Application.Features.Admin.Commands.BlockEvent;
using EventHub.Application.Exceptions;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.BlockEvent;

public class BlockEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Event>> _eventRepositoryMock;
    private readonly BlockEventCommandHandler _handler;

    public BlockEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventRepositoryMock = new Mock<IGenericRepository<Event>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);

        _handler = new BlockEventCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEventId_ShouldBlockEventAndSave()
    {
        var eventId = Guid.NewGuid();
        var command = new BlockEventCommand(eventId, "Kršenje pravila platforme");

        var existingEvent = new Event(); 

        _eventRepositoryMock
            .Setup(repo => repo.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();

        _eventRepositoryMock.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidEventId_ShouldThrowNotFoundException()
    {
        var eventId = Guid.NewGuid();
        var command = new BlockEventCommand(eventId, "Razlog");

        _eventRepositoryMock
            .Setup(repo => repo.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}