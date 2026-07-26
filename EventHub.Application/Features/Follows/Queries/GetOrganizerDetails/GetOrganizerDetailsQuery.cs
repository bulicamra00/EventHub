using MediatR;
using System;

namespace EventHub.Application.Features.Follows.Queries.GetOrganizerDetails;

public record GetOrganizerDetailsQuery(Guid Id) : IRequest<OrganizerDetailsDto>;