using MediatR;
using EventHub.Application.Features.Events.Queries.GetEvents; 

namespace EventHub.Application.Features.Events.Queries.GetMyInvitations;

public class GetMyInvitationsQuery : IRequest<List<EventDto>>
{
}