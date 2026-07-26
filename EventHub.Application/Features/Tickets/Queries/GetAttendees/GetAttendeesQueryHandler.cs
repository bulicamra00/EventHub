using EventHub.Application.Features.Tickets.Queries.GetAttendees;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Enums;
using MediatR;

namespace EventHub.Application.Features.Tickets.Queries.GetAttendees;

public class GetAttendeesQueryHandler : IRequestHandler<GetAttendeesQuery, List<AttendeeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAttendeesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AttendeeDto>> Handle(GetAttendeesQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _unitOfWork.Tickets.GetListByConditionAsync(
            t => t.EventId == request.EventId, 
            "TicketType"
        );

        return tickets.Select(t => new AttendeeDto
        {
            TicketId = t.Id,
            AttendeeName = t.AttendeeName,
            AttendeeEmail = t.AttendeeEmail,
            TicketCode = t.TicketCode,
            TicketTypeName = t.TicketType?.Name ?? "N/A", 
            PurchaseDate = t.PurchaseDate,
            Status = t.Status.ToString(),
            IsScanned = t.Status == TicketStatus.Used
        }).ToList();
    }
}