using EventHub.Application.Features.Tickets.Queries.GetAttendees;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Tickets.Queries.GetAttendees;

public class GetAttendeesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;

    private readonly GetAttendeesQueryHandler _handler;

    public GetAttendeesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();

        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);

        _handler = new GetAttendeesQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTicketsExistForEvent()
    {
        var eventId = Guid.NewGuid();
        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.Is<string>(s => s == "TicketType")))
            .ReturnsAsync(new List<Ticket>());

        var query = new GetAttendeesQuery(eventId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnAttendeesList_WhenTicketsExist()
    {
        var eventId = Guid.NewGuid();
        var purchaseDate = DateTime.UtcNow;

        var ticket = new Ticket
        {
            EventId = eventId,
            AttendeeName = "Petar Petrovic",
            AttendeeEmail = "petar@example.com",
            TicketCode = "TCK-12345",
            PurchaseDate = purchaseDate,
            Status = TicketStatus.Active,
            TicketType = new TicketType { Name = "VIP" }
        };
        SetProperty(ticket, "Id", Guid.NewGuid());

        var ticketsList = new List<Ticket> { ticket };

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.Is<string>(s => s == "TicketType")))
            .ReturnsAsync(ticketsList);

        var query = new GetAttendeesQuery(eventId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        result[0].TicketId.Should().Be(ticket.Id);
        result[0].AttendeeName.Should().Be("Petar Petrovic");
        result[0].AttendeeEmail.Should().Be("petar@example.com");
        result[0].TicketCode.Should().Be("TCK-12345");
        result[0].TicketTypeName.Should().Be("VIP");
        result[0].PurchaseDate.Should().Be(purchaseDate);
        result[0].Status.Should().Be("Active");
        result[0].IsScanned.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnNA_AndIsScannedTrue_WhenTicketTypeIsNullAndStatusIsUsed()
    {
        var eventId = Guid.NewGuid();

        var ticket = new Ticket
        {
            EventId = eventId,
            AttendeeName = "Jovana Jovanovic",
            AttendeeEmail = "jovana@example.com",
            TicketCode = "TCK-99999",
            PurchaseDate = DateTime.UtcNow,
            Status = TicketStatus.Used,
            TicketType = null!
        };
        SetProperty(ticket, "Id", Guid.NewGuid());

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.Is<string>(s => s == "TicketType")))
            .ReturnsAsync(new List<Ticket> { ticket });

        var query = new GetAttendeesQuery(eventId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].TicketTypeName.Should().Be("N/A");
        result[0].Status.Should().Be("Used");
        result[0].IsScanned.Should().BeTrue();
    }

    private void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName) ?? obj.GetType().BaseType?.GetProperty(propertyName);
        property?.SetValue(obj, value);
    }
}