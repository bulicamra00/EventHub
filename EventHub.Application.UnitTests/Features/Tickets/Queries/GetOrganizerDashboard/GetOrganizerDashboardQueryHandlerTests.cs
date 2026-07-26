using EventHub.Application.Features.Tickets.Queries.GetOrganizerDashboard;
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

namespace EventHub.Application.UnitTests.Features.Tickets.Queries.GetOrganizerDashboard;

public class GetOrganizerDashboardQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepoMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypesRepoMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;

    private readonly GetOrganizerDashboardQueryHandler _handler;

    public GetOrganizerDashboardQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventsRepoMock = new Mock<IGenericRepository<Event>>();
        _ticketTypesRepoMock = new Mock<IGenericRepository<TicketType>>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypesRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);

        _handler = new GetOrganizerDashboardQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyDto_WhenSpecificEventNotFound()
    {
        var organizerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _eventsRepoMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event)null!);

        var query = new GetOrganizerDashboardQuery(organizerId, eventId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalTicketsSold.Should().Be(0);
        result.TotalRevenue.Should().Be(0);
        result.TotalCancelledTickets.Should().Be(0);
        result.CapacityUtilizationPercentage.Should().Be(0);
        result.TicketTypeStats.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyDto_WhenEventDoesNotBelongToOrganizer()
    {
        var organizerId = Guid.NewGuid();
        var otherOrganizerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var eventEntity = new Event { OrganizerId = otherOrganizerId };
        SetProperty(eventEntity, "Id", eventId);

        _eventsRepoMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        var query = new GetOrganizerDashboardQuery(organizerId, eventId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalTicketsSold.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyDto_WhenOrganizerHasNoEvents()
    {
        var organizerId = Guid.NewGuid();

        _eventsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(new List<Event>());

        var query = new GetOrganizerDashboardQuery(organizerId, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalTicketsSold.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnDashboardStats_WhenEventsAndTicketsExist()
    {
        var organizerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var eventEntity = new Event { OrganizerId = organizerId };
        SetProperty(eventEntity, "Id", eventId);

        _eventsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(new List<Event> { eventEntity });

        var vipTicketType = new TicketType { EventId = eventId, Name = "VIP" };
        SetProperty(vipTicketType, "SoldCount", 10);
        SetProperty(vipTicketType, "Capacity", 20);

        var regularTicketType = new TicketType { EventId = eventId, Name = "Regular" };
        SetProperty(regularTicketType, "SoldCount", 40);
        SetProperty(regularTicketType, "Capacity", 80);

        var ticketTypes = new List<TicketType> { vipTicketType, regularTicketType };

        _ticketTypesRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<TicketType, bool>>>()))
            .ReturnsAsync(ticketTypes);

        var tickets = new List<Ticket>
        {
            new Ticket { EventId = eventId, Status = TicketStatus.Active, PurchasePrice = 100m },
            new Ticket { EventId = eventId, Status = TicketStatus.Used, PurchasePrice = 50m },
            new Ticket { EventId = eventId, Status = TicketStatus.Cancelled, PurchasePrice = 50m }
        };

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .ReturnsAsync(tickets);

        var query = new GetOrganizerDashboardQuery(organizerId, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalTicketsSold.Should().Be(2);
        result.TotalRevenue.Should().Be(150m);
        result.TotalCancelledTickets.Should().Be(1);
        result.CapacityUtilizationPercentage.Should().Be(2.0); 
        
        result.TicketTypeStats.Should().HaveCount(2);
        result.TicketTypeStats.Should().Contain(s => s.TicketTypeName == "VIP" && s.SoldCount == 10 && s.TotalCapacity == 20);
        result.TicketTypeStats.Should().Contain(s => s.TicketTypeName == "Regular" && s.SoldCount == 40 && s.TotalCapacity == 80);
    }

    private void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName) ?? obj.GetType().BaseType?.GetProperty(propertyName);
        property?.SetValue(obj, value);
    }
}