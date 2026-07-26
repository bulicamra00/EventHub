using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Admin.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.CompleteAsync();

        return category.Id; 
    }
}