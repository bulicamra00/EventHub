using EventHub.Application.Features.Tickets.Commands.ScanTicket;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Tickets.Commands.ScanTicket;

public class ScanTicketCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Ticket>> _ticketRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ScanTicketCommandHandler _handler;

    public ScanTicketCommandHandlerTests()
    {
        _ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new ScanTicketCommandHandler(_ticketRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTicketNotFound()
    {
        var command = new ScanTicketCommand(Guid.NewGuid().ToString(), Guid.NewGuid());

        _ticketRepositoryMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .ReturnsAsync((Ticket?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _ticketRepositoryMock.Verify(r => r.Update(It.IsAny<Ticket>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Theory]
    [InlineData(TicketStatus.Used)]
    [InlineData(TicketStatus.Cancelled)]
    public async Task Handle_ShouldReturnFalse_WhenTicketIsAlreadyUsedOrCancelled(TicketStatus status)
    {
        var command = new ScanTicketCommand(Guid.NewGuid().ToString(), Guid.NewGuid());
        var ticket = new Ticket { Status = status };

        _ticketRepositoryMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .ReturnsAsync(ticket);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _ticketRepositoryMock.Verify(r => r.Update(It.IsAny<Ticket>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldScanSuccessfullyAndReturnTrue_WhenTicketIsValid()
    {
        var command = new ScanTicketCommand(Guid.NewGuid().ToString(), Guid.NewGuid());
        var ticket = new Ticket { Status = TicketStatus.Active }; 

        _ticketRepositoryMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .ReturnsAsync(ticket);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Used);
        _ticketRepositoryMock.Verify(r => r.Update(ticket), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}