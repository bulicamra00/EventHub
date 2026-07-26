using AutoMapper;
using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Application.Features.Events.Queries.GetMyInvitations;
using EventHub.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace EventHub.Application.Features.Events.Queries.GetMyInvitations;

public class GetMyInvitationsQueryHandler : IRequestHandler<GetMyInvitationsQuery, List<EventDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetMyInvitationsQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<List<EventDto>> Handle(GetMyInvitationsQuery request, CancellationToken ct)
    {
        var email = _currentUserService.Email;
        
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException("Korisnik nije ulogovan.");
        }

        var invitations = await _unitOfWork.EventInvitations.GetListByConditionAsync(
            i => i.Email == email, 
            "Event.Organizer", 
            "Event.Category" 
        );

        return _mapper.Map<List<EventDto>>(invitations);
    }
}