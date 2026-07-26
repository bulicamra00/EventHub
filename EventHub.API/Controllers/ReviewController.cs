using EventHub.Application.Features.Reviews.Commands.CreateReview;
using EventHub.Application.Features.Reviews.Queries.GetEventReviews; 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator) => _mediator = mediator;

    
    [HttpGet("event/{eventId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewDto>>> GetEventReviews(Guid eventId)
    {
        var result = await _mediator.Send(new GetEventReviewsQuery(eventId));
        return Ok(result);
    }

    
    [Authorize(Roles = "Attendee")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
    {
        var reviewId = await _mediator.Send(command);
        return Ok(new 
        { 
            ReviewId = reviewId, 
            Message = "Vaša ocena je uspešno sačuvana. Hvala na povratnoj informaciji!" 
        });
    }
}