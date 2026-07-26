using MediatR;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Enums;

namespace EventHub.Application.Features.Events.Commands.CreateRecurringEvent;

public class CreateRecurringEventCommandHandler : IRequestHandler<CreateRecurringEventCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateRecurringEventCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateRecurringEventCommand request, CancellationToken ct)
    {
        var organizerId = _currentUserService.UserId;
        if (organizerId == null)
        {
            throw new UnauthorizedAccessException("Morate biti ulogovani kao organizator da biste kreirali događaj.");
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            throw new Exception($"Kategorija nije pronađena.");
        }

        var series = new EventSeries
        {
            Name = request.Title,
            Description = request.Description,
            RecurrencePattern = "Weekly",
            EndDate = request.StartDate.AddDays(request.NumberOfWeeks * 7)
        };

        await _unitOfWork.EventSeries.AddAsync(series);

        for (int i = 0; i < request.NumberOfWeeks; i++)
        {
            var eventInstance = new Event
            {
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate.AddDays(i * 7),
                Category = category, 
                Location = request.Location,
                EventSeries = series,
                OrganizerId = organizerId.Value
            };


            await _unitOfWork.Events.AddAsync(eventInstance);
        }

        await _unitOfWork.CompleteAsync();

        return series.Id;
    }
}