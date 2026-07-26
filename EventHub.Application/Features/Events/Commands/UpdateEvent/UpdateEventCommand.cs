using MediatR;
using System.Text.Json.Serialization;
using EventHub.Application.Common; 

namespace EventHub.Application.Features.Events.Commands.UpdateEvent;

public record UpdateEventCommand(
    [property: JsonIgnore] Guid Id,
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
) : IRequest<Unit>;