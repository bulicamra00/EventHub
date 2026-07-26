using EventHub.Application.Features.Reviews.Commands.CreateReview;
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

namespace EventHub.Application.UnitTests.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Ticket>> _ticketsRepoMock;
    private readonly Mock<IGenericRepository<Review>> _reviewsRepoMock;

    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _ticketsRepoMock = new Mock<IGenericRepository<Ticket>>();
        _reviewsRepoMock = new Mock<IGenericRepository<Review>>();

        _unitOfWorkMock.Setup(u => u.Tickets).Returns(_ticketsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Reviews).Returns(_reviewsRepoMock.Object);

        _handler = new CreateReviewCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotLoggedIn()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var command = new CreateReviewCommand(Guid.NewGuid(), 5, "Odličan događaj!");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Korisnik nije ulogovan.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserHasNoUsedTicketForEvent()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .ReturnsAsync(new List<Ticket>());

        var command = new CreateReviewCommand(Guid.NewGuid(), 4, string.Empty);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Možete oceniti samo događaj kojem ste prisustvovali (karta mora biti skenirana).");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserHasAlreadyReviewedEvent()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var usedTicket = new Ticket { UserId = userId, EventId = eventId, Status = TicketStatus.Used };
        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.Is<Expression<Func<Ticket, bool>>>(expr => true)))
            .ReturnsAsync(new List<Ticket> { usedTicket });

        var existingReview = new Review { UserId = userId, EventId = eventId };
        _reviewsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.Is<Expression<Func<Review, bool>>>(expr => true)))
            .ReturnsAsync(new List<Review> { existingReview });

        var command = new CreateReviewCommand(eventId, 5, string.Empty);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Već ste ocenili ovaj događaj.");
    }

    [Fact]
    public async Task Handle_ShouldCreateReviewAndReturnId_WhenValidRequest()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var usedTicket = new Ticket { UserId = userId, EventId = eventId, Status = TicketStatus.Used };
        _ticketsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.Is<Expression<Func<Ticket, bool>>>(expr => true)))
            .ReturnsAsync(new List<Ticket> { usedTicket });

        _reviewsRepoMock
            .Setup(r => r.GetListByConditionAsync(It.Is<Expression<Func<Review, bool>>>(expr => true)))
            .ReturnsAsync(new List<Review>()); 

        Review? addedReview = null;
        _reviewsRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Review>()))
            .Callback<Review>(r => addedReview = r)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var command = new CreateReviewCommand(eventId, 5, "Sve preporuke!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        
        addedReview.Should().NotBeNull();
        addedReview!.EventId.Should().Be(eventId);
        addedReview.UserId.Should().Be(userId);
        addedReview.Rating.Should().Be(5);
        addedReview.Comment.Should().Be("Sve preporuke!");

        _reviewsRepoMock.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}