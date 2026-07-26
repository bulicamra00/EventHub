using MediatR;

namespace EventHub.Application.Features.Admin.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Description) : IRequest<Guid>;