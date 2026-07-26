using EventHub.Application.Features.Follows.Commands.FollowOrganizer;
using EventHub.Application.Features.Follows.Commands.UnfollowOrganizer; 
using EventHub.Application.Features.Follows.Queries.GetFollowedOrganizers;
using EventHub.Application.Features.Follows.Queries.GetOrganizers;
using EventHub.Application.Features.Follows.Queries.GetOrganizerDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class FollowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowsController(IMediator mediator) => _mediator = mediator;

    
    [Authorize(Roles = "Attendee")]
    [HttpPost("follow/{organizerId}")]
    public async Task<IActionResult> FollowOrganizer(Guid organizerId)
    {
        var command = new FollowOrganizerCommand(organizerId);
        var result = await _mediator.Send(command);
        
        return Ok(new 
        { 
            Success = result, 
            Message = "Organizator je uspešno zapraćen." 
        });
    }

    
    [Authorize(Roles = "Attendee")]
    [HttpDelete("unfollow/{organizerId}")]
    public async Task<IActionResult> UnfollowOrganizer(Guid organizerId)
    {
        var command = new UnfollowOrganizerCommand(organizerId);
        var result = await _mediator.Send(command);
        
        return Ok(new 
        { 
            Success = result, 
            Message = "Organizator je uspešno otpraćen." 
        });
    }

    
    [HttpGet("organizers")]
    public async Task<IActionResult> GetAllOrganizers()
    {
        var query = new GetOrganizersQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    
    [HttpGet("organizers/{id}")]
    public async Task<IActionResult> GetOrganizerDetails(Guid id)
    {
        var query = new GetOrganizerDetailsQuery(id);
        var result = await _mediator.Send(query);
        
        return result != null ? Ok(result) : NotFound($"Organizator sa ID-jem {id} nije pronađen.");
    }

    
    [Authorize(Roles = "Attendee")]
    [HttpGet("followed-organizers")]
    public async Task<IActionResult> GetFollowedOrganizers()
    {
        var query = new GetFollowedOrganizersQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}