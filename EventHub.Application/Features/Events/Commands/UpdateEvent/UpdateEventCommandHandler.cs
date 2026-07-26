using MediatR;
using EventHub.Application.Features.Events.Commands.UpdateEvent;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public UpdateEventCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(UpdateEventCommand request, CancellationToken ct)
    {
        if (_currentUserService.Role != "Organizer")
        {
            throw new UnauthorizedAccessException("Samo organizatori mogu menjati događaje.");
        }

        var existingEvent = await _unitOfWork.Events.GetByIdAsync(request.Id);
        
        if (existingEvent == null)
            throw new Exception($"Događaj sa ID {request.Id} nije pronađen.");

        if (existingEvent.OrganizerId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("Nemate dozvolu da menjate ovaj događaj.");

        var oldStartDate = existingEvent.StartDate;
        var oldLocation = existingEvent.Location;

        existingEvent.Title = request.Title;
        existingEvent.Description = request.Description;
        existingEvent.StartDate = request.StartDate;
        existingEvent.EndDate = request.EndDate;
        existingEvent.Location = request.Location;
        existingEvent.Latitude = request.Latitude;
        existingEvent.Longitude = request.Longitude;
        existingEvent.OnlineLink = request.OnlineLink;
        existingEvent.CoverImageUrl = request.CoverImageUrl;
        existingEvent.CategoryId = request.CategoryId;
        existingEvent.IsPrivate = request.IsPrivate;

        var oldEventTags = await _unitOfWork.EventTags.GetListByConditionAsync(et => et.EventId == request.Id);
        _unitOfWork.EventTags.RemoveRange(oldEventTags);

        foreach (var tagName in request.TagNames)
        {
            var tag = await _unitOfWork.Tags.GetByConditionAsync(t => t.Name == tagName) 
                    ?? new Tag { Name = tagName };
            
            if (tag.Id == Guid.Empty) await _unitOfWork.Tags.AddAsync(tag);

            await _unitOfWork.EventTags.AddAsync(new EventTag { EventId = existingEvent.Id, Tag = tag });
        }

        var existingTicketTypes = await _unitOfWork.TicketTypes.GetListByConditionAsync(t => t.EventId == request.Id);

        foreach (var ticketDto in request.TicketTypes)
        {
            var existingType = existingTicketTypes.FirstOrDefault(t => t.Name.Trim().ToLower() == ticketDto.Name.Trim().ToLower());

            if (existingType != null)
            {
                existingType.Price = ticketDto.Price;
                existingType.UpdateCapacity(ticketDto.Capacity);
                
                _unitOfWork.TicketTypes.Update(existingType);
            }
            else
            {
                var newTicketType = new TicketType(ticketDto.Name, ticketDto.Capacity, ticketDto.Price)
                {
                    EventId = existingEvent.Id
                };
                await _unitOfWork.TicketTypes.AddAsync(newTicketType);
            }
        }

        bool hasTimeOrLocationChanged = oldStartDate != request.StartDate || oldLocation != request.Location;

        if (hasTimeOrLocationChanged)
        {
            var tickets = await _unitOfWork.Tickets.GetListByConditionAsync(t => 
                t.EventId == request.Id && t.Status != TicketStatus.Cancelled, "User");

            foreach (var ticket in tickets)
            {
                if (ticket.User == null || string.IsNullOrEmpty(ticket.User.Email)) continue;

                var changeDetails = $"Datum/vreme početka: {request.StartDate:dd.MM.yyyy HH:mm}, Lokacija: {request.Location}";
                var emailBody = $"Poštovani, došlo je do izmena za događaj '{existingEvent.Title}'.\n\nNovi detalji:\n{changeDetails}";
                
                await _emailService.SendEmailAsync(ticket.User.Email, "Važna izmena detalja događaja", emailBody);

                var notification = new Notification
                {
                    UserId = ticket.UserId,
                    Message = $"Događaj '{existingEvent.Title}' je izmenjen. Novi detalji - {changeDetails}"
                };
                
                await _unitOfWork.Notifications.AddAsync(notification);
            }
        }

        await _unitOfWork.CompleteAsync();

        return Unit.Value;
    }
}