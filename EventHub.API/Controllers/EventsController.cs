using EventHub.Application.Features.Events.Commands.CreateEvent;
using EventHub.Application.Features.Events.Commands.CreateRecurringEvent;
using EventHub.Application.Features.Events.Commands.PublishEvent;
using EventHub.Application.Features.Events.Commands.CancelEvent;
using EventHub.Application.Features.Events.Commands.UpdateEvent;
using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Application.Features.Events.Queries.GetOrganizerEvents;
using EventHub.Application.Features.Events.Queries.GetPersonalizedEvents;
using EventHub.Application.Features.Events.Queries.GetEventDetails;
using EventHub.Application.Features.Events.Queries.GetMyInvitations;
using EventHub.Application.Features.Tickets.Queries.GetOrganizerDashboard;
using EventHub.Application.Features.Tickets.Queries.GetAttendees;
using EventHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace EventHub.API.Controllers;

public record CancelEventRequest(string Reason);

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = "Organizer")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
    {
        var eventId = await _mediator.Send(command);
        return Ok(new { EventId = eventId, Message = "Događaj je uspešno kreiran!" });
    }

    [Authorize(Roles = "Organizer")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventCommand command)
    {
        
        var commandWithId = command with { Id = id };
        
        await _mediator.Send(commandWithId);
        return Ok(new { Message = "Događaj je uspešno ažuriran!" });
    }

    [Authorize(Roles = "Organizer")]
    [HttpPost("recurring")]
    public async Task<IActionResult> CreateRecurring([FromBody] CreateRecurringEventCommand command)
    {
        var seriesId = await _mediator.Send(command);
        return Ok(new { SeriesId = seriesId, Message = "Serija događaja je uspešno kreirana!" });
    }

    [Authorize(Roles = "Organizer")]
    [HttpPatch("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await _mediator.Send(new PublishEventCommand(id));
        if (!result) return BadRequest("Događaj nije pronađen ili se ne može objaviti.");
        return Ok(new { Message = "Događaj je uspešno objavljen!" });
    }

    [Authorize(Roles = "Organizer")]
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelEventRequest request)
    {
        var result = await _mediator.Send(new CancelEventCommand(id, request.Reason));
        if (!result) return BadRequest("Otkazivanje nije uspelo.");
        return Ok(new { Message = "Događaj je uspešno otkazan i obaveštenja su poslata." });
    }

    [Authorize(Roles = "Organizer")]
    [HttpGet("my-events")]
    public async Task<IActionResult> GetMyEvents(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] EventStatus? status = null)
    {
        var result = await _mediator.Send(new GetOrganizerEventsQuery(pageNumber, pageSize, status));
        
        return Ok(new 
        { 
            Data = result.Items,
            Meta = new { result.TotalCount, PageNumber = pageNumber, PageSize = pageSize }
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] string? token)
    {
        var result = await _mediator.Send(new GetEventDetailsQuery(id, token));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? city,
        [FromQuery] DateTime? startDate,
        [FromQuery] List<Guid>? tagIds,
        [FromQuery] EventStatus? status,
        [FromQuery] bool? onlyRecurring,
        [FromQuery] double? userLatitude,
        [FromQuery] double? userLongitude,
        [FromQuery] double? radiusKm,
        [FromQuery] string? sortBy,         
        [FromQuery] bool? descending,       
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetEventsQuery(
            SearchTerm: searchTerm,
            CategoryId: categoryId,
            City: city,
            StartDate: startDate,
            TagIds: tagIds,
            UserLatitude: userLatitude,
            UserLongitude: userLongitude,
            RadiusKm: radiusKm,
            Status: status,
            OnlyRecurring: onlyRecurring ?? false,
            SortBy: sortBy,                 
            Descending: descending ?? false,
            PageNumber: pageNumber,
            PageSize: pageSize
        );
        
        var result = await _mediator.Send(query);
        return Ok(new 
        { 
            Data = result.Items,
            Meta = new 
            { 
                TotalCount = result.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize)
            }
        });
    }

    [Authorize(Roles = "Attendee")]
    [HttpGet("my-invitations")]
    public async Task<IActionResult> GetMyInvitations()
    {
        var result = await _mediator.Send(new GetMyInvitationsQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Attendee")]
    [HttpGet("personalized")]
    public async Task<IActionResult> GetPersonalized()
    {
        var result = await _mediator.Send(new GetPersonalizedEventsQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Organizer")]
    [HttpGet("{eventId}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid eventId)
    {
        var result = await _mediator.Send(new GetOrganizerDashboardQuery(eventId));
        return Ok(result);
    }

    [Authorize(Roles = "Organizer")]
    [HttpGet("{eventId}/attendees")]
    public async Task<IActionResult> GetAttendees(Guid eventId)
    {
        var result = await _mediator.Send(new GetAttendeesQuery(eventId));
        return Ok(result);
    }
}