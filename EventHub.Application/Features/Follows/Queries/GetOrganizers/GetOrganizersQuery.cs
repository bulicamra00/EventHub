using MediatR;
using System.Collections.Generic;

namespace EventHub.Application.Features.Follows.Queries.GetOrganizers;

public record GetOrganizersQuery : IRequest<List<OrganizerSummaryDto>>;