using AutoMapper;
using EventHub.Application.Features.Tickets.Queries.GetMyTickets;
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

namespace EventHub.Application.UnitTests.Features.Tickets.Queries.GetMyTickets;

public class GetMyTicketsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly GetMyTicketsQueryHandler _handler;

    public GetMyTicketsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);

        _handler = new GetMyTicketsQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserIsNotLoggedIn()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var query = new GetMyTicketsQuery();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Korisnik nije ulogovan.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserIdIsEmpty()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.Empty);

        var query = new GetMyTicketsQuery();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Korisnik nije ulogovan.");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTicketsExistForUser()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.Is<string>(s => s == "Event")))
            .ReturnsAsync(new List<Ticket>());

        _mapperMock
            .Setup(m => m.Map<List<TicketDto>>(It.IsAny<List<Ticket>>()))
            .Returns(new List<TicketDto>());

        var query = new GetMyTicketsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedTickets_WhenTicketsExistForUser()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var ticketsList = new List<Ticket>
        {
            new Ticket { UserId = userId, EventId = Guid.NewGuid() }
        };

        var ticketDtosList = new List<TicketDto>
        {
            new TicketDto { TicketCode = "TCK-11111" }
        };

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.Is<string>(s => s == "Event")))
            .ReturnsAsync(ticketsList);

        _mapperMock
            .Setup(m => m.Map<List<TicketDto>>(ticketsList))
            .Returns(ticketDtosList);

        var query = new GetMyTicketsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].TicketCode.Should().Be("TCK-11111");

        _mapperMock.Verify(m => m.Map<List<TicketDto>>(ticketsList), Times.Once);
    }
}