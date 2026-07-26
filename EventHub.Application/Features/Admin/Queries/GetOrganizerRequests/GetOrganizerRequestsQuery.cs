using MediatR;
using System.Collections.Generic;

namespace EventHub.Application.Features.Admin.Queries.GetOrganizerRequests;

public class GetOrganizerRequestsQuery : IRequest<IEnumerable<OrganizerRequestDto>>
{
}