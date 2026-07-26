using EventHub.Application.Features.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System.Threading.Tasks;

namespace EventHub.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator) => _mediator = mediator;

    
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var query = new GetCategoriesQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}