using MediatR;
using System;

namespace EventHub.Application.Features.Admin.Commands.ApproveOrganizerRequest;

public record ApproveOrganizerRequestCommand(Guid UserId) : IRequest<Unit>;