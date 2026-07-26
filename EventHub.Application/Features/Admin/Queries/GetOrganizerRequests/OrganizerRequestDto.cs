using System;

namespace EventHub.Application.Features.Admin.Queries.GetOrganizerRequests;

public class OrganizerRequestDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? OrganizerRequestStatus { get; set; }
}