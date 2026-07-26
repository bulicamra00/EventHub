using EventHub.Application.Features.Events.Commands.CreateRecurringEvent;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Commands.CreateRecurringEvent;

public class CreateRecurringEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<Category>> _categoriesRepositoryMock;
    private readonly Mock<IGenericRepository<EventSeries>> _eventSeriesRepositoryMock;
    private readonly Mock<IGenericRepository<Event>> _eventsRepositoryMock;
    private readonly CreateRecurringEventCommandHandler _handler;

    public CreateRecurringEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _categoriesRepositoryMock = new Mock<IGenericRepository<Category>>();
        _eventSeriesRepositoryMock = new Mock<IGenericRepository<EventSeries>>();
        _eventsRepositoryMock = new Mock<IGenericRepository<Event>>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoriesRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EventSeries).Returns(_eventSeriesRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventsRepositoryMock.Object);

        _handler = new CreateRecurringEventCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndCategoryExists_ShouldCreateSeriesAndEventsAndReturnSeriesId()
    {
        var organizerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(s => s.UserId).Returns(organizerId);

        var category = new Category();
        typeof(Category).GetProperty(nameof(Category.Id))?.SetValue(category, categoryId);

        _categoriesRepositoryMock.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        _eventSeriesRepositoryMock.Setup(r => r.AddAsync(It.IsAny<EventSeries>()))
            .Callback<EventSeries>(s => typeof(EventSeries).GetProperty(nameof(EventSeries.Id))?.SetValue(s, seriesId))
            .Returns(Task.CompletedTask);

        var command = new CreateRecurringEventCommand
        {
            Title = "Weekly Meetup",
            Description = "A regular sync meeting",
            StartDate = DateTime.UtcNow.AddDays(1),
            NumberOfWeeks = 3,
            CategoryId = categoryId,
            Location = "Online"
        };

        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(seriesId);

        _eventSeriesRepositoryMock.Verify(r => r.AddAsync(It.Is<EventSeries>(series => 
            series.Name == command.Title && 
            series.Description == command.Description &&
            series.RecurrencePattern == "Weekly")), 
            Times.Once);

        _eventsRepositoryMock.Verify(r => r.AddAsync(It.Is<Event>(ev => 
            ev.Title == command.Title && 
            ev.OrganizerId == organizerId &&
            ev.Category == category)), 
            Times.Exactly(3));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var command = new CreateRecurringEventCommand
        {
            Title = "Weekly Meetup",
            Description = "A regular sync meeting",
            StartDate = DateTime.UtcNow.AddDays(1),
            NumberOfWeeks = 3,
            CategoryId = Guid.NewGuid(),
            Location = "Online"
        };

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Morate biti ulogovani kao organizator da biste kreirali događaj.");

        _categoriesRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _eventSeriesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<EventSeries>()), Times.Never);
        _eventsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ShouldThrowException()
    {
        var organizerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(organizerId);
        _categoriesRepositoryMock.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        var command = new CreateRecurringEventCommand
        {
            Title = "Weekly Meetup",
            Description = "A regular sync meeting",
            StartDate = DateTime.UtcNow.AddDays(1),
            NumberOfWeeks = 3,
            CategoryId = categoryId,
            Location = "Online"
        };

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Kategorija nije pronađena.");

        _eventSeriesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<EventSeries>()), Times.Never);
        _eventsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }
}