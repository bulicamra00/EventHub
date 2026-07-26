using EventHub.Application.Features.Users.Queries.GetUserProfile;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Enums;
using AutoMapper;
using MediatR;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("Korisnik nije ulogovan.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception($"Korisnik sa ID-em {userId} nije pronađen.");
        }

        var bookings = await _unitOfWork.Bookings.GetListByConditionAsync(b => b.UserId == userId);
        
        var attendedTickets = await _unitOfWork.Tickets.GetListByConditionAsync(
            t => t.UserId == userId && t.Status == TicketStatus.Used, 
            "Event"
        );

        var attendedEvents = attendedTickets.Select(t => new AttendedEventDto(
            t.Event.Id,
            t.Event.Title,
            t.Event.StartDate
        )).ToList();

        var interestsList = !string.IsNullOrWhiteSpace(user.Interests) 
            ? user.Interests.Split(',').Select(i => i.Trim()).ToList() 
            : new List<string>();
        
        var userDto = new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.City ?? string.Empty,
            user.CreatedAt,
            interestsList,
            bookings.Count(),
            attendedEvents
        );

        return userDto;
    }
}