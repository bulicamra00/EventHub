using EventHub.Application.Features.Admin.Commands.BlockEvent;
using EventHub.Application.Features.Admin.Commands.BlockUser;
using EventHub.Application.Features.Admin.Commands.CreateCategory;
using EventHub.Application.Features.Admin.Commands.UnblockEvent;
using EventHub.Application.Features.Admin.Commands.UnblockUser;
using EventHub.Application.Features.Admin.Commands.ApproveOrganizerRequest;
using EventHub.Application.Features.Admin.Queries.GetCategories;
using EventHub.Application.Features.Admin.Queries.GetUsers;
using EventHub.Application.Features.Admin.Queries.GetOrganizerRequests;
using EventHub.Application.Features.Admin.Queries.GetPlatformStats; 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    
    [Authorize(Roles = "Admin")]
    [HttpGet("stats")]
    public async Task<IActionResult> GetPlatformStats()
    {
        var stats = await _mediator.Send(new GetPlatformStatsQuery());
        return Ok(stats);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _mediator.Send(new GetUsersQuery());
        return Ok(users);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("organizer-requests")]
    public async Task<IActionResult> GetOrganizerRequests()
    {
        var requests = await _mediator.Send(new GetOrganizerRequestsQuery());
        return Ok(requests);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("users/{userId}/approve-organizer")]
    public async Task<IActionResult> ApproveOrganizer(Guid userId)
    {
        await _mediator.Send(new ApproveOrganizerRequestCommand(userId));
        
        return Ok(new 
        { 
            Message = $"Zahtev korisnika sa ID-jem {userId} je uspešno odobren." 
        });
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("users/{userId}/block")]
    public async Task<IActionResult> BlockUser(Guid userId, [FromBody] string reason)
    {
        await _mediator.Send(new BlockUserCommand(userId, reason));
        
        return Ok(new 
        { 
            Message = $"Korisnik sa ID-jem {userId} je uspešno blokiran zbog: {reason}" 
        });
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("users/{userId}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid userId)
    {
        await _mediator.Send(new UnblockUserCommand(userId));
        
        return Ok(new 
        { 
            Message = $"Korisnik sa ID-jem {userId} je uspešno odblokiran." 
        });
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("events/{eventId}/block")]
    public async Task<IActionResult> BlockEvent(Guid eventId, [FromBody] string reason)
    {
        await _mediator.Send(new BlockEventCommand(eventId, reason));
        
        return Ok(new 
        { 
            Message = $"Događaj sa ID-jem {eventId} je uspešno blokiran zbog: {reason}" 
        });
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("events/{eventId}/unblock")]
    public async Task<IActionResult> UnblockEvent(Guid eventId)
    {
        await _mediator.Send(new UnblockEventCommand(eventId));
        
        return Ok(new 
        { 
            Message = $"Događaj sa ID-jem {eventId} je uspešno odblokiran." 
        });
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _mediator.Send(new GetCategoriesQuery());
        return Ok(categories);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var categoryId = await _mediator.Send(command);
        
        return Ok(new 
        { 
            CategoryId = categoryId, 
            Message = "Kategorija je uspešno kreirana." 
        });
    }
}