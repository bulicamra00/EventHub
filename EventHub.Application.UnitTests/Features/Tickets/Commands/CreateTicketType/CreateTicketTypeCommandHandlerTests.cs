using EventHub.Application.Features.Tickets.Commands.CreateTicketType;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Tickets.Commands.CreateTicketType;

public class CreateTicketTypeCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypesRepoMock;
    private readonly CreateTicketTypeCommandHandler _handler;

    public CreateTicketTypeCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _ticketTypesRepoMock = new Mock<IGenericRepository<TicketType>>();

        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypesRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        _handler = new CreateTicketTypeCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTicketTypeSuccessfully_WhenValidRequest()
    {
        var eventId = Guid.NewGuid();
        var command = new CreateTicketTypeCommand(
            eventId,
            "VIP",
            1000m,
            800m,
            DateTime.UtcNow.AddDays(7),
            50
        );

        TicketType? capturedTicketType = null;
        _ticketTypesRepoMock
            .Setup(r => r.AddAsync(It.IsAny<TicketType>()))
            .Callback<TicketType>(t => capturedTicketType = t)
            .Returns(Task.CompletedTask);

        var resultId = await _handler.Handle(command, CancellationToken.None);

        resultId.Should().NotBeEmpty();
        capturedTicketType.Should().NotBeNull();
        capturedTicketType!.Name.Should().Be(command.Name);
        capturedTicketType.Price.Should().Be(command.Price);
        capturedTicketType.Capacity.Should().Be(command.Capacity);
        capturedTicketType.EventId.Should().Be(command.EventId);
        capturedTicketType.EarlyBirdPrice.Should().Be(command.EarlyBirdPrice);
        capturedTicketType.EarlyBirdExpiryDate.Should().Be(command.EarlyBirdExpiryDate);

        _ticketTypesRepoMock.Verify(r => r.AddAsync(It.IsAny<TicketType>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}