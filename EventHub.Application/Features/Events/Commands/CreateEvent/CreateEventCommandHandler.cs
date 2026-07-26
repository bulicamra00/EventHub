using MediatR;
using EventHub.Application.Features.Events.Commands.CreateEvent;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventHub.Application.Features.Events.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateEventCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CreateEventCommandHandler(
        IUnitOfWork unitOfWork, 
        ILogger<CreateEventCommandHandler> logger, 
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken ct)
    {
        if (_currentUserService.Role != "Organizer")
        {
            throw new UnauthorizedAccessException("Samo organizatori mogu kreirati događaje.");
        }

        var organizerId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("Korisnik nije pronađen.");

        var newEvent = new Event
        {
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            OnlineLink = request.OnlineLink,
            CoverImageUrl = request.CoverImageUrl,
            CategoryId = request.CategoryId,
            OrganizerId = organizerId,
            IsPrivate = request.IsPrivate
        };

        await _unitOfWork.Events.AddAsync(newEvent);
        await _unitOfWork.CompleteAsync(); 

        if (request.TagNames != null && request.TagNames.Any())
        {
            foreach (var tagName in request.TagNames)
            {
                var tag = await _unitOfWork.Tags.GetByConditionAsync(t => t.Name == tagName);
                
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    await _unitOfWork.Tags.AddAsync(tag);
                    await _unitOfWork.CompleteAsync(); 
                }

                await _unitOfWork.EventTags.AddAsync(new EventTag 
                { 
                    EventId = newEvent.Id, 
                    TagId = tag.Id 
                });
            }
        }

        if (request.TicketTypes != null && request.TicketTypes.Any())
        {
            foreach (var ticketDto in request.TicketTypes)
            {
                var ticketType = new TicketType(ticketDto.Name, ticketDto.Capacity, ticketDto.Price)
                {
                    EventId = newEvent.Id
                };
                
                await _unitOfWork.TicketTypes.AddAsync(ticketType);
            }
        }

        await _unitOfWork.CompleteAsync(); 

        _logger.LogInformation("Event {EventId} je uspešno kreiran (Privatan: {IsPrivate}).", newEvent.Id, newEvent.IsPrivate);

        return newEvent.Id;
    }
}