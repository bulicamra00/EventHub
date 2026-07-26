using MediatR;
using AutoMapper;
using EventHub.Domain.Interfaces;
using EventHub.Application.Features.Events.Queries.GetOrganizerEvents;
using EventHub.Application.Features.Events.Queries.GetEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Events.Queries.GetOrganizerEvents;

public class GetOrganizerEventsQueryHandler : IRequestHandler<GetOrganizerEventsQuery, OrganizerEventsResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _userService;

    public GetOrganizerEventsQueryHandler(IUnitOfWork uow, IMapper mapper, ICurrentUserService userService)
    {
        _unitOfWork = uow;
        _mapper = mapper;
        _userService = userService;
    }

    public async Task<OrganizerEventsResponse> Handle(GetOrganizerEventsQuery request, CancellationToken ct)
    {
        var organizerId = _userService.UserId;

        var query = _unitOfWork.Events.GetQueryable("Category")
            .Where(e => e.OrganizerId == organizerId);

        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        var totalCount = query.Count();
        
        var items = query
            .OrderByDescending(e => e.StartDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<EventDto>>(items);
        
        return new OrganizerEventsResponse(dtos, totalCount);
    }
}