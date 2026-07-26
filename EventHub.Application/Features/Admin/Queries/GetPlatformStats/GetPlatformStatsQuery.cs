using MediatR;

namespace EventHub.Application.Features.Admin.Queries.GetPlatformStats;

public record GetPlatformStatsQuery() : IRequest<PlatformStatsDto>;