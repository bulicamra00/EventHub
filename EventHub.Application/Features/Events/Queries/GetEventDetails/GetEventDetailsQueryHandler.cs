using AutoMapper;
using EventHub.Application.Features.Events.Queries.GetEventDetails;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Events.Queries.GetEventDetails;

public class GetEventDetailsQueryHandler : IRequestHandler<GetEventDetailsQuery, EventDetailsDto>
{
    private readonly IGenericRepository<Event> _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetEventDetailsQueryHandler(
        IGenericRepository<Event> eventRepository, 
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<EventDetailsDto> Handle(GetEventDetailsQuery request, CancellationToken cancellationToken)
    {
        var query = _eventRepository.GetQueryable("Category", "TicketTypes", "EventTags.Tag");
        var eventEntity = query.FirstOrDefault(e => e.Id == request.Id);

        if (eventEntity == null)
        {
            throw new Exception($"Event sa ID {request.Id} nije pronađen.");
        }

        if (eventEntity.IsPrivate)
        {
            var isOrganizer = eventEntity.OrganizerId == _currentUserService.UserId;

            var invitation = await _unitOfWork.EventInvitations.GetByConditionAsync(i => 
                i.EventId == eventEntity.Id && 
                i.Email == _currentUserService.Email);

            bool hasValidInvitation = (invitation != null);

            if (!isOrganizer && !hasValidInvitation)
            {
                throw new UnauthorizedAccessException("Ovaj događaj je privatan i niste autorizovani da ga vidite.");
            }
        }

        var dto = _mapper.Map<EventDetailsDto>(eventEntity);
        
        if (_currentUserService.UserId != null)
        {
            var ticket = await _unitOfWork.Tickets.GetByConditionAsync(t => 
                t.EventId == eventEntity.Id && t.UserId == _currentUserService.UserId);
            
            dto.UserHasTicket = (ticket != null);
        }
        else
        {
            dto.UserHasTicket = false;
        }

        return dto;
    }
}