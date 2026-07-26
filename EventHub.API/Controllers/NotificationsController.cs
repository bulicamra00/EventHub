using EventHub.Application.Features.Notifications.Queries.GetMyNotifications;
using EventHub.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using EventHub.Application.Features.Notifications.Commands.SendNotification; 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var notifications = await _mediator.Send(new GetMyNotificationsQuery());
        return Ok(notifications);
    }

    
    [Authorize]
    [HttpPost("read/{id}")]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid id)
    {
        var success = await _mediator.Send(new MarkNotificationAsReadCommand(id));
        
        if (!success)
        {
            return NotFound(new { Message = "Notifikacija nije pronađena." });
        }
        
        return Ok(new { Message = "Notifikacija je uspešno označena kao pročitana." });
    }

    
    [Authorize(Roles = "Organizer,Admin")]
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
    {
        var success = await _mediator.Send(command);

        if (!success)
        {
            return BadRequest(new { Message = "Slanje obaveštenja nije uspelo ili nema prijavljenih učesnika za ovaj događaj." });
        }

        return Ok(new { Message = "Obaveštenje je uspešno poslato svim učesnicima!" });
    }
}