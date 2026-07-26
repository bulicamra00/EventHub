using EventHub.Domain.Entities;
using MediatR;

namespace EventHub.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<User>>;