using EventHub.Application.Features.Events.Commands.CreateInvitation;
using EventHub.Application.Features.Events.Commands.AcceptInvitation; 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsController(IMediator mediator) => _mediator = mediator;

    
    [Authorize(Roles = "Organizer")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationCommand command)
    {
        var invitationId = await _mediator.Send(command);
        
        return Ok(new 
        { 
            InvitationId = invitationId, 
            Message = "Pozivnica je uspešno kreirana i poslata na navedenu e-mail adresu." 
        });
    }

    
    [HttpPost("accept/{token}")]
    public async Task<IActionResult> AcceptInvitation([FromRoute] string token)
    {
        var command = new AcceptInvitationCommand(token);
        var success = await _mediator.Send(command);
        
        if (!success)
        {
            return BadRequest(new { Message = "Pozivnica nije validna ili je već iskorišćena." });
        }
        
        return Ok(new { Message = "Pozivnica je uspešno prihvaćena." });
    }
}