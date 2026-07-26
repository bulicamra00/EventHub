using MediatR;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Enums;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Admin.Queries.GetPlatformStats;

public class GetPlatformStatsQueryHandler : IRequestHandler<GetPlatformStatsQuery, PlatformStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPlatformStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PlatformStatsDto> Handle(GetPlatformStatsQuery request, CancellationToken cancellationToken)
    {
        
        var users = await _unitOfWork.Users.GetAllAsync();
        var usersList = users.ToList();

        var totalUsers = usersList.Count;
        var totalOrganizers = usersList.Count(u => u.Role == UserRole.Organizer);
        var totalAttendees = usersList.Count(u => u.Role == UserRole.Attendee);

        var events = await _unitOfWork.Events.GetAllAsync();
        var eventsList = events.ToList();

        var totalEvents = eventsList.Count;
        var publishedEvents = eventsList.Count(e => e.Status == EventStatus.Published);

        var tickets = await _unitOfWork.Tickets.GetAllAsync();
        var ticketsList = tickets.ToList();

        var totalTicketsSold = ticketsList.Count;
        var totalRevenue = ticketsList.Sum(t => t.PurchasePrice); 

        return new PlatformStatsDto
        {
            TotalUsers = totalUsers,
            TotalOrganizers = totalOrganizers,
            TotalAttendees = totalAttendees,
            TotalEvents = totalEvents,
            PublishedEvents = publishedEvents,
            TotalTicketsSold = totalTicketsSold,
            TotalRevenue = totalRevenue
        };
    }
}