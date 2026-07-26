using EventHub.Application.Features.Bookings.Commands.CreateBooking;
using EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;
using EventHub.Application.Features.Bookings.Commands.CancelBooking; 
using EventHub.Application.Features.Bookings.Queries.GetMyBookings;
using EventHub.Application.Features.Bookings.Queries.GetBookingById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator) => _mediator = mediator;

    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery(id));
        
        if (result == null)
            return NotFound(new { Message = "Rezervacija nije pronađena." });
            
        return Ok(result);
    }

    
    [Authorize] 
    [HttpPost("create")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var bookingId = await _mediator.Send(command);
        return Ok(new { BookingId = bookingId, Message = "Rezervacija je uspešno kreirana! Molimo izvršite plaćanje u roku od 10 minuta." });
    }

    
    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        await _mediator.Send(new CancelBookingCommand { BookingId = id });
        return Ok(new { Message = "Rezervacija je uspešno otkazana." });
    }

    
    [Authorize]
    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetMyBookingsQuery(userId));
        return Ok(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("cleanup")]
    public async Task<IActionResult> ManualCleanup()
    {
        await _mediator.Send(new CancelExpiredBookingsCommand());
        return Ok(new { Message = "Ručno čišćenje isteklih rezervacija je pokrenuto." });
    }
}