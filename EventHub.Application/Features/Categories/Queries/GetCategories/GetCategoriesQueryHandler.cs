using MediatR;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        
        return categories.Select(c => new CategoryDto 
        { 
            Id = c.Id, 
            Name = c.Name 
        }).ToList();
    }
}