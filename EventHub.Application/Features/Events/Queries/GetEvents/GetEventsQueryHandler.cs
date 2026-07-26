using MediatR;
using AutoMapper;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EventHub.Application.Features.Events.Queries.GetEvents;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, (IEnumerable<EventDto> Items, int TotalCount)> 
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEventsQueryHandler(IUnitOfWork uow, IMapper mapper) 
    { 
        _unitOfWork = uow; 
        _mapper = mapper; 
    }

    public async Task<(IEnumerable<EventDto> Items, int TotalCount)> Handle(GetEventsQuery request, CancellationToken ct) 
    {
        var query = _unitOfWork.Events.GetQueryable("Category", "EventTags.Tag", "Organizer", "TicketTypes");

        query = query.Where(e => e.Status != EventStatus.Draft && !e.IsPrivate && !e.IsBlocked);

        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        if (request.OnlyRecurring)
        {
            query = query.Where(e => e.EventSeriesId != null);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e => 
                e.Title.ToLower().Contains(term) ||
                (e.Description != null && e.Description.ToLower().Contains(term)) ||
                (e.Category != null && e.Category.Name.ToLower().Contains(term)) ||
                e.EventTags.Any(et => et.Tag.Name.ToLower().Contains(term))
            );
        }

        if (request.CategoryId.HasValue)
            query = query.Where(e => e.CategoryId == request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(e => e.Location != null && e.Location.Contains(request.City));

        if (request.StartDate.HasValue)
            query = query.Where(e => e.StartDate >= request.StartDate.Value);

        if (request.TagIds != null && request.TagIds.Any())
            query = query.Where(e => e.EventTags.Any(et => request.TagIds.Contains(et.TagId)));

        if (request.UserLatitude.HasValue && request.UserLongitude.HasValue && request.RadiusKm.HasValue)
        {
            var lat = request.UserLatitude.Value;
            var lon = request.UserLongitude.Value;
            var radius = request.RadiusKm.Value;

            query = query.Where(e => e.Latitude.HasValue && e.Longitude.HasValue &&
                (6371 * Math.Acos(
                    Math.Cos(Math.PI * lat / 180) * Math.Cos(Math.PI * e.Latitude.Value / 180) * Math.Cos(Math.PI * (e.Longitude.Value - lon) / 180) + 
                    Math.Sin(Math.PI * lat / 180) * Math.Sin(Math.PI * e.Latitude.Value / 180)
                )) <= radius);
        }

        var sortByClean = request.SortBy?.Trim().ToLower();

        query = sortByClean switch
        {
            "popularity" or "attendees" => request.Descending 
                ? query.OrderByDescending(e => e.TicketTypes.Sum(tt => tt.SoldCount + tt.ReservedCount)) 
                : query.OrderBy(e => e.TicketTypes.Sum(tt => tt.SoldCount + tt.ReservedCount)),
                
            "date" => request.Descending 
                ? query.OrderByDescending(e => e.StartDate) 
                : query.OrderBy(e => e.StartDate),
                
            _ => request.Descending 
                ? query.OrderByDescending(e => e.StartDate) 
                : query.OrderBy(e => e.StartDate) 
        };

        var totalCount = query.Count(); 
        
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList(); 
        
        var eventDtos = _mapper.Map<IEnumerable<EventDto>>(items);

        return (eventDtos, totalCount);
    }
}