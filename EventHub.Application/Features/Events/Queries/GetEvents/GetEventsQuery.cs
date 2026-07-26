using MediatR;
using EventHub.Domain.Enums;

namespace EventHub.Application.Features.Events.Queries.GetEvents;

public record GetEventsQuery(
    string? SearchTerm = null,
    Guid? CategoryId = null,
    string? City = null,
    DateTime? StartDate = null,
    List<Guid>? TagIds = null,
    double? UserLatitude = null,
    double? UserLongitude = null,
    double? RadiusKm = null,
    EventStatus? Status = null,
    bool OnlyRecurring = false,
    string? SortBy = null,
    bool Descending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<(IEnumerable<EventDto> Items, int TotalCount)>;