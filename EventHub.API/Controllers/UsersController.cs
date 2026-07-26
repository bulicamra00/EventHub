using EventHub.Application.Features.Users.Commands.RegisterUser;
using EventHub.Application.Features.Users.Commands.ConfirmEmail;
using EventHub.Application.Features.Users.Commands.LoginUser;
using EventHub.Application.Features.Users.Commands.RefreshToken;
using EventHub.Application.Features.Users.Commands.RevokeToken;
using EventHub.Application.Features.Users.Commands.UpdateUser;
using EventHub.Application.Features.Users.Commands.RequestOrganizer; 
using EventHub.Application.Features.Users.Queries.GetUserProfile;
using EventHub.Application.Features.Users.Queries.GetOrganizerProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    
    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var result = await _mediator.Send(new GetUserProfileQuery());
        return Ok(result);
    }

    
    [Authorize(Roles = "Organizer")]
    [HttpGet("organizer-profile")]
    public async Task<ActionResult<OrganizerProfileDto>> GetOrganizerProfile()
    {
        var result = await _mediator.Send(new GetOrganizerProfileQuery());
        return Ok(result);
    }

    
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Profil je uspešno ažuriran!" });
    }

    
    [Authorize]
    [HttpPost("request-organizer")]
    public async Task<IActionResult> RequestOrganizer()
    {
        await _mediator.Send(new RequestOrganizerCommand());
        return Ok(new { Message = "Zahtev za organizatora je uspešno poslat." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var userId = await _mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Uspešno registrovano, proverite email!" });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand(token));

        if (!result)
            return BadRequest(new { Message = "Neuspešna potvrda. Token je istekao ili je nevažeći." });

        return Ok(new { Message = "Email je uspešno potvrđen! Sada se možete prijaviti." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}