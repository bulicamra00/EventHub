using AutoMapper;
using EventHub.Application.Features.Tickets.Queries.GetMyTickets;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Tickets.Queries.GetMyTickets;

public class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, List<TicketDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyTicketsQueryHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService, 
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<TicketDto>> Handle(GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId == null || userId == Guid.Empty)
        {
            throw new Exception("Korisnik nije ulogovan.");
        }

        var tickets = await _unitOfWork.Tickets.GetListByConditionAsync(
            t => t.UserId == userId.Value, 
            "Event" 
        );

        var ticketDtos = _mapper.Map<List<TicketDto>>(tickets);

        return ticketDtos;
    }
}