using EventHub.Application.Features.Tickets.Queries.GetOrganizerDashboard;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Tickets.Queries.GetOrganizerDashboard;

public class GetOrganizerDashboardQueryHandler : IRequestHandler<GetOrganizerDashboardQuery, OrganizerDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrganizerDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrganizerDashboardDto> Handle(GetOrganizerDashboardQuery request, CancellationToken cancellationToken)
    {
        List<Guid> eventIds;
        
        if (request.EventId.HasValue)
        {
            var eventExists = await _unitOfWork.Events.GetByIdAsync(request.EventId.Value);
            if (eventExists == null || eventExists.OrganizerId != request.OrganizerId)
            {
                return new OrganizerDashboardDto();
            }
            eventIds = new List<Guid> { request.EventId.Value };
        }
        else
        {
            var events = await _unitOfWork.Events.GetListByConditionAsync(e => e.OrganizerId == request.OrganizerId);
            eventIds = events.Select(e => e.Id).ToList();
        }

        if (!eventIds.Any())
        {
            return new OrganizerDashboardDto();
        }

        var allTicketTypes = await _unitOfWork.TicketTypes.GetListByConditionAsync(tt => eventIds.Contains(tt.EventId));
        var allTickets = await _unitOfWork.Tickets.GetListByConditionAsync(t => eventIds.Contains(t.EventId));

        var soldTickets = allTickets.Where(t => t.Status != TicketStatus.Cancelled);
        
        var dto = new OrganizerDashboardDto
        {
            TotalTicketsSold = soldTickets.Count(),
            TotalRevenue = soldTickets.Sum(t => t.PurchasePrice),
            TotalCancelledTickets = allTickets.Count(t => t.Status == TicketStatus.Cancelled),
            
            TicketTypeStats = allTicketTypes.GroupBy(tt => tt.Name)
                .Select(g => new TicketTypeStatisticsDto
                {
                    TicketTypeName = g.Key,
                    SoldCount = g.Sum(x => x.SoldCount),
                    TotalCapacity = g.Sum(x => x.Capacity)
                }).ToList()
        };

        int totalCapacity = allTicketTypes.Sum(tt => tt.Capacity);
        
        dto.CapacityUtilizationPercentage = totalCapacity > 0 
            ? (double)dto.TotalTicketsSold / totalCapacity * 100 
            : 0;

        return dto;
    }
}