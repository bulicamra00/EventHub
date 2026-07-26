using EventHub.Application.Features.Tickets.Commands.CancelTicket;
using EventHub.Application.Features.Tickets.Commands.CreateTicketType;
using EventHub.Application.Features.Tickets.Commands.PurchaseTicket;
using EventHub.Application.Features.Tickets.Commands.ScanTicket;
using EventHub.Application.Features.Tickets.Queries.GetMyTickets;
using EventHub.Application.Features.Tickets.Queries.GetAttendees;
using EventHub.Application.Features.Tickets.Queries.GetOrganizerDashboard;
using EventHub.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICsvService _csvService;

    public TicketsController(IMediator mediator, ICsvService csvService)
    {
        _mediator = mediator;
        _csvService = csvService;
    }

    [HttpPost("types")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> CreateType([FromBody] CreateTicketTypeCommand command)
    {
        var ticketTypeId = await _mediator.Send(command);
        return Ok(new { TicketTypeId = ticketTypeId, Message = "Tip karte je uspešno kreiran!" });
    }

    [HttpPost("purchase")]
    [Authorize(Roles = "Attendee")]
    public async Task<IActionResult> Purchase([FromBody] PurchaseTicketCommand command)
    {
        var ticketId = await _mediator.Send(command);
        return Ok(new { TicketId = ticketId, Message = "Karta je uspešno kupljena!" });
    }

    [HttpGet("my-tickets")]
    [Authorize(Roles = "Attendee")]
    public async Task<IActionResult> GetMyTickets()
    {
        var tickets = await _mediator.Send(new GetMyTicketsQuery());
        return Ok(tickets);
    }

    [HttpPost("scan")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> ScanTicket([FromBody] ScanTicketCommand command)
    {
        var success = await _mediator.Send(command);
        if (!success)
            return BadRequest(new { Message = "Skeniranje nije uspelo: Karta je nevažeća, već iskorišćena ili otkazana." });

        return Ok(new { Message = "Karta je uspešno skenirana i validirana!" });
    }

    
    [HttpGet("{eventId}/attendees")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetAttendees(Guid eventId)
    {
        var attendees = await _mediator.Send(new GetAttendeesQuery(eventId));
        return Ok(attendees);
    }

    [HttpGet("{eventId}/export-csv")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> ExportAttendeesCsv(Guid eventId)
    {
        var attendees = await _mediator.Send(new GetAttendeesQuery(eventId));
        var fileContents = _csvService.ExportAttendeesToCsv(attendees);

        return File(fileContents, "text/csv", $"Ucesnici_Dogadjaj_{eventId}.csv");
    }

    
    [HttpGet("stats/global")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetGlobalOrganizerStats()
    {
        return await GetStatsInternal(null);
    }

    
    [HttpGet("stats/{eventId}")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetEventStats(Guid eventId)
    {
        return await GetStatsInternal(eventId);
    }

  
    private async Task<IActionResult> GetStatsInternal(Guid? eventId)
    {
        var organizerIdClaim = User.FindFirst("sub")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(organizerIdClaim) || !Guid.TryParse(organizerIdClaim, out Guid organizerId))
            return Unauthorized(new { Message = "Korisnik nije prepoznat ili ID nije validan." });

        var stats = await _mediator.Send(new GetOrganizerDashboardQuery(organizerId, eventId));
        return Ok(stats);
    }

    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = "Attendee")]
    public async Task<IActionResult> CancelTicket(Guid id)
    {
        var success = await _mediator.Send(new CancelTicketCommand(id));
        
        if (!success)
            return BadRequest(new { Message = "Otkazivanje nije uspelo: Proverite status karte ili politiku otkazivanja (moguće samo 24h pre događaja)." });

        return Ok(new { Message = "Karta je uspešno otkazana." });
    }
}