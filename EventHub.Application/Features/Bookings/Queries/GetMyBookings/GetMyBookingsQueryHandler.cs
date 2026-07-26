using AutoMapper;
using EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Bookings.Queries.GetMyBookings;

public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, List<BookingDto>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetMyBookingsQueryHandler(IMediator mediator, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BookingDto>> Handle(GetMyBookingsQuery request, CancellationToken ct)
    {
        await _mediator.Send(new CancelExpiredBookingsCommand(), ct);

        var userIdGuid = Guid.Parse(request.UserId);
        
        var bookings = await _unitOfWork.Bookings.GetListByConditionAsync(
            b => b.UserId == userIdGuid && b.Status != BookingStatus.Confirmed, 
            "Event" 
        );

        return _mapper.Map<List<BookingDto>>(bookings);
    }
}