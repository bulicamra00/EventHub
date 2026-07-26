using MediatR;
using EventHub.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace EventHub.Application.Features.Admin.Queries.GetOrganizerRequests;

public class GetOrganizerRequestsQueryHandler : IRequestHandler<GetOrganizerRequestsQuery, IEnumerable<OrganizerRequestDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrganizerRequestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OrganizerRequestDto>> Handle(GetOrganizerRequestsQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync(); 
        
        var requests = users
            .Where(u => u.IsOrganizerRequested)
            .Select(u => new OrganizerRequestDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                City = u.City,
                OrganizerRequestStatus = u.OrganizerRequestStatus
            })
            .ToList();

        return requests;
    }
}