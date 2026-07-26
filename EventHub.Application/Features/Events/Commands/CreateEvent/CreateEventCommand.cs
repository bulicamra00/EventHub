using MediatR;
using EventHub.Application.Common; 

namespace EventHub.Application.Features.Events.Commands.CreateEvent;

public record CreateEventCommand(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    double? Latitude,      
    double? Longitude,     
    string? OnlineLink,
    string? CoverImageUrl, 
    Guid CategoryId,
    bool IsPrivate,
    List<string> TagNames, 
    List<TicketTypeDto> TicketTypes 
) : IRequest<Guid>;