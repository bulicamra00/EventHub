using EventHub.Application.Features.Tickets.Commands.PurchaseTicket;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Tickets.Commands.PurchaseTicket;

public class PurchaseTicketCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IJobService> _jobServiceMock;
    private readonly Mock<IGenericRepository<User>> _usersRepoMock;
    private readonly Mock<IGenericRepository<TicketType>> _ticketTypesRepoMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;

    private readonly PurchaseTicketCommandHandler _handler;

    public PurchaseTicketCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _jobServiceMock = new Mock<IJobService>();
        _usersRepoMock = new Mock<IGenericRepository<User>>();
        _ticketTypesRepoMock = new Mock<IGenericRepository<TicketType>>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_usersRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.TicketTypes).Returns(_ticketTypesRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        _handler = new PurchaseTicketCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _jobServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotLoggedIn()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var command = new PurchaseTicketCommand(Guid.NewGuid(), 1, "John Doe", "john@example.com");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Korisnik nije ulogovan.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserNotFoundInDb()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var command = new PurchaseTicketCommand(Guid.NewGuid(), 1, "John Doe", "john@example.com");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Korisnik nije pronađen u sistemu.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTicketTypeNotFound()
    {
        var userId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        
        var user = new User { FullName = "Pera Perić", Email = "pera@example.com" };
        SetProperty(user, "Id", userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        _ticketTypesRepoMock.Setup(r => r.GetByIdAsync(ticketTypeId)).ReturnsAsync((TicketType?)null);

        var command = new PurchaseTicketCommand(ticketTypeId, 1, "John Doe", "john@example.com");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Tip karte nije pronađen.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNotEnoughCapacity()
    {
        var userId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var user = new User { FullName = "Pera Perić", Email = "pera@example.com" };
        SetProperty(user, "Id", userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var ticketType = new TicketType("VIP", 0, 100m);
        SetProperty(ticketType, "Id", ticketTypeId);
        _ticketTypesRepoMock.Setup(r => r.GetByIdAsync(ticketTypeId)).ReturnsAsync(ticketType);

        var command = new PurchaseTicketCommand(ticketTypeId, 1, "John Doe", "john@example.com");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Nažalost, nema dovoljno dostupnih karata za ovaj događaj.");
    }

    [Fact]
    public async Task Handle_ShouldPurchaseTicketSuccessfully_WhenValidRequest()
    {
        var userId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var user = new User { FullName = "Pera Perić", Email = "pera@example.com" };
        SetProperty(user, "Id", userId);
        _usersRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var ticketType = new TicketType("VIP", 50, 100m);
        SetProperty(ticketType, "Id", ticketTypeId);
        SetProperty(ticketType, "EventId", Guid.NewGuid());
        
        ticketType.Reserve(2);

        _ticketTypesRepoMock.Setup(r => r.GetByIdAsync(ticketTypeId)).ReturnsAsync(ticketType);

        Ticket? capturedTicket = null;
        _ticketsRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Ticket>()))
            .Callback<Ticket>(t => capturedTicket = t)
            .Returns(Task.CompletedTask);

        var command = new PurchaseTicketCommand(ticketTypeId, 2, "John Doe", "john@example.com");

        var resultId = await _handler.Handle(command, CancellationToken.None);

        resultId.Should().NotBeEmpty();
        capturedTicket.Should().NotBeNull();
        capturedTicket!.UserId.Should().Be(userId);
        capturedTicket.AttendeeName.Should().Be("Pera Perić"); 
        capturedTicket.AttendeeEmail.Should().Be("pera@example.com");
        capturedTicket.Status.Should().Be(TicketStatus.PendingPayment);

        _ticketsRepoMock.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        _jobServiceMock.Verify(j => j.EnqueuePaymentProcessing(capturedTicket.Id), Times.Once);
    }

    private void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName) ?? obj.GetType().BaseType?.GetProperty(propertyName);
        property?.SetValue(obj, value);
    }
}