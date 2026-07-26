using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Admin.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<Category>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<Category>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return (await _unitOfWork.Categories.GetAllAsync()).ToList();
    }
}