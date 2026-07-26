using MediatR;

namespace EventHub.Application.Features.Tickets.Queries.GetOrganizerDashboard;

public record GetOrganizerDashboardQuery(Guid OrganizerId, Guid? EventId = null) : IRequest<OrganizerDashboardDto>;