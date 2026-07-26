using MediatR;

namespace EventHub.Application.Features.Tickets.Commands.CreateTicketType;

public record CreateTicketTypeCommand(
    Guid EventId,
    string Name,
    decimal Price,
    decimal? EarlyBirdPrice,
    DateTime? EarlyBirdExpiryDate,
    int Capacity
) : IRequest<Guid>;