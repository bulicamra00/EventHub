using EventHub.Domain.Entities;
using MediatR;

namespace EventHub.Application.Features.Admin.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<List<Category>>;